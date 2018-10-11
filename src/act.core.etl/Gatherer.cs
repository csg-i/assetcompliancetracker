using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using act.core.data;
using act.core.etl.ComplianceModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Environment = act.core.data.Environment;


namespace act.core.etl 
{
    class Gatherer : IGatherer
    {
        
        private readonly ActDbContext _ctx;
        private readonly ILogger _logger;
        private readonly MailSettings _mailSettings;

        public Gatherer(ActDbContext ctx, ILoggerFactory logger, IConfiguration configuration)
        {
            _ctx = ctx;    
            _mailSettings = new MailSettings();
            configuration.GetSection("Mail").Bind(_mailSettings);
            _logger = logger.CreateLogger<Gatherer>();
        }

        private static void SetHeaders(HttpClient client, Environment environment)
        {
            client.BaseAddress = new Uri(environment.ChefAutomateUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Clear();
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.AcceptEncoding.Clear();

            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.8));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.8));
            client.DefaultRequestHeaders.Add("chef-delivery-enterprise", environment.ChefAutomateOrg);
            client.DefaultRequestHeaders.Add("x-data-collector-auth", "version=1.0");
            client.DefaultRequestHeaders.Add("x-data-collector-token", environment.ChefAutomateToken);
        }

        private static readonly HashSet<string> PciProfiles = new HashSet<string>{"csg_windows_compliant_server", "csg_linux_compliant_server"};
        private static readonly HashSet<string> ShouldNotControls = new HashSet<string> { "csg-windows-7.3", "csg-windows-7.1", "csg-linux-7.3" };
        private static readonly HashSet<string> ErrorControls = new HashSet<string> { "csg-windows-7.5", "csg-windows-7.6" };
        private static readonly HashSet<string> ShouldControls = new HashSet<string> { "csg-windows-7.4", "csg-windows-7.2", "csg-linux-7.4" };
        private static readonly HashSet<string> PortControls = new HashSet<string> { "csg-windows-12", "csg-linux-12" };
        private static readonly HashSet<string> OsVersionControls = new HashSet<string> { "csg-windows-1", "csg-linux-1" };

        private static IEnumerable<ComplianceResultTest> ParsePorts(ComplianceReportProfile profile, long id)
        {
            var control = profile.controls.FirstOrDefault(p => PortControls.Contains(p.id));
            if (control?.results == null) yield break;
            foreach (var result in control.results)
            {
                var type = result.GetPortType();
                var status = result.GetComplainceStatus();
                if(status == ComplianceStatusConstant.Failed)
                    yield return new ComplianceResultTest
                    {
                        ComplianceResultId = id,
                        Name = result.GetPortNumber(),
                        ResultType = JustificationTypeConstant.Port,
                        PortType = type,
                        ShouldExist = false,
                        Status = ComplianceStatusConstant.Failed
                    };
            }
        }
      
        private static bool ParseOsTest(ComplianceReportProfile profile)
        {
            var control = profile.controls.FirstOrDefault(p => OsVersionControls.Contains(p.id));
            if (control != null)
                return control.results.All(p => p.GetComplainceStatus() == ComplianceStatusConstant.Succeeded);

            return true;
        }

       
        private static IEnumerable<ComplianceResultTest> ParseSoftwareShouldNots(ComplianceReportProfile profile, long id)
        {
            var results = profile.controls.Where(p => ShouldNotControls.Contains(p.id)).Where(p=>p.results != null).SelectMany(p=>p.results);
            return ParseSoftware(results, false, id);            
        }
        private static IEnumerable<ComplianceResultTest> ParseSoftwareShoulds(ComplianceReportProfile profile, long id)
        {
            var results = profile.controls.Where(p => ShouldControls.Contains(p.id)).Where(p => p.results != null).SelectMany(p => p.results);
            return ParseSoftware(results, true, id);
        }

        private static IEnumerable<ComplianceResultError> ParseErrors(ComplianceReportProfile profile, long id)
        {
            var controls = profile.controls.Where(p => ErrorControls.Contains(p.id)).Where(p => p.results != null);
            foreach (var control in controls)
            {
                foreach (var result in control.results)
                {
                    var status = result.GetComplainceStatus();
                    if (status == ComplianceStatusConstant.Failed)
                    {
                        yield return new ComplianceResultError
                        {
                            ComplianceResultId = id,
                            Name = control.desc,
                            Code = result.code_desc,
                            LongMessage  = result.message,
                            Status = ComplianceStatusConstant.Failed
                        };
                    }
                }
            }
        }
        private static IEnumerable<ComplianceResultTest> ParseSoftware(IEnumerable<ComplianceReportProfileControlResult> results, bool shouldExist, long id)
        {
            foreach (var result in results)
            {
                var status = result.GetComplainceStatus();
                if (status == ComplianceStatusConstant.Failed)
                {
                    yield return new ComplianceResultTest
                    {
                        ComplianceResultId = id,
                        Name = result.GetSoftwareName(),
                        ResultType = result.GetSoftwareJustificationType(),
                        ShouldExist = shouldExist,
                        Status = ComplianceStatusConstant.Failed
                    };
                }
            }
        }

        public async Task SaveComplianceDataFromWebHook(AutomateWebHookMessage message)
        {
            var node = await _ctx.Nodes.Active().ById(message.node_uuid);
            var text = node == null ? "did not find" : "found";
            _logger.LogTrace($"NotifyComplianceFailure WebHook got a compliance failure message for chef node id {message.node_uuid} and {text} a matching node {node?.Fqdn}.");
            if (node != null)
            {
                var run = await SaveComplianceData(node.InventoryItemId, message);

                if (run != null)
                {
                    node.LastComplianceResultDate = run.EndTime;
                    node.LastComplianceResultId = run.ResultId;
                    if (!node.FailingSince.HasValue)
                        node.FailingSince = run.EndTime;
                    node.ComplianceStatus = run.Status;
                    await _ctx.SaveChangesAsync();
                }
            }
        }

        private async Task<ComplianceResult> SaveComplianceData(long inventoryItemId, IComplianceReport rpt)
        {
            var csgProfile = rpt?.profiles.FirstOrDefault(p => PciProfiles.Contains(p.name ?? string.Empty));
            if (csgProfile == null || await _ctx.ComplianceResults.Exists(inventoryItemId, rpt.id)) 
                return null;
            
            var run = _ctx.ComplianceResults.Add(new ComplianceResult
            {
                InventoryItemId = inventoryItemId,
                ResultId = rpt.id,
                EndTime = rpt.end_time,
                EndDate = rpt.end_time.Date,
                OperatingSystemTestPassed = ParseOsTest(csgProfile),
                Status = rpt.GetComplianceStatus(),
            }).Entity;
            
            await _ctx.SaveChangesAsync(); //save to get Id

            var failedTests = ParseSoftwareShouldNots(csgProfile, run.Id).ToList();
            failedTests.AddRange(ParseSoftwareShoulds(csgProfile, run.Id));
            failedTests.AddRange(ParsePorts(csgProfile, run.Id));
            var errors = ParseErrors(csgProfile, run.Id).ToArray();

            foreach (var p in failedTests)
            {
                _ctx.ComplianceResultTests.Add(p);
            }

            foreach (var p in errors)
            {
                _ctx.ComplianceResultErrors.Add(p);
            }

            if (run.OperatingSystemTestPassed && !failedTests.Any() && !errors.Any() &&
                run.Status == ComplianceStatusConstant.Failed)
                run.Status = ComplianceStatusConstant.Succeeded;

            await _ctx.SaveChangesAsync();

            return run;
        }

        public async Task ComplianceReport(long nodeId)
        {
            await ComplianceReport(await _ctx.Nodes.ById(nodeId));
        }
        
        private async Task ComplianceReport(Node node)
        {
            if (node?.LastComplianceResultId != null && node.ChefNodeId.HasValue &&
                ! (await _ctx.ComplianceResults.Exists(node.InventoryItemId, node.LastComplianceResultId.Value)))
            {
                var rpt = await GetReport(node.EnvironmentId, node.LastComplianceResultId.Value);

                var result = await SaveComplianceData(node.InventoryItemId, rpt);
                node.ComplianceStatus = result.Status;
                await _ctx.SaveChangesAsync();
            }

        }
        
        public async Task<HttpResponseMessage> Proxy(int environmentId, string url)
        {
            using (var client = new HttpClient())
            {
                SetHeaders(client, await _ctx.Environments.ById(environmentId));
                return await client.GetAsync(url);
            }
        }
        private async Task<ComplianceReport> GetReport(int environmentId, Guid latestReportId)
        {
            using (var client = new HttpClient())
            {
                SetHeaders(client, await _ctx.Environments.ById(environmentId));

                var result = await client.GetAsync(BuildReportsUrl(latestReportId));
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<ComplianceReport>(json);
                    return obj;
                }
                return null;
            }
        }
        public async Task<ComplianceNode[]> Gather(int environmentId, Guid[] nodeIds = null)
        {
            var list = new List<ComplianceNode>();
            using (var client = new HttpClient())
            {
                SetHeaders(client, await _ctx.Environments.ById(environmentId));

                var result = await client.GetAsync(BuildNodesUrl(100, 1, nodeIds));

                var countHeader = result.Headers.Where(p => p.Key == "X-Total-Count").Select(p => p.Value)
                    .FirstOrDefault()?.FirstOrDefault();
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<ComplianceNode[]>(json);
                    var saveResult = await SaveAndReturnFailuresAndFound(obj);
                    list.AddRange(saveResult.Item2);
                    var found = saveResult.Item1;
                    
                    if (!string.IsNullOrWhiteSpace(countHeader) && int.TryParse(countHeader, out var totalCount))
                    {
                        _logger.LogDebug($"Updated {found} of {totalCount} nodes from automate.");
                        var pageCount = totalCount / 100 + ((totalCount % 100) > 0 ? 1 : 0);
                        for (var p = 2; p <= pageCount; p++)
                        {
                            result = await client.GetAsync(BuildNodesUrl(100, p, nodeIds));
                            if (result.IsSuccessStatusCode)
                            {
                                json = await result.Content.ReadAsStringAsync();
                                obj = JsonConvert.DeserializeObject<ComplianceNode[]>(json);
                                saveResult = await SaveAndReturnFailuresAndFound(obj);
                                list.AddRange(saveResult.Item2);
                                found += saveResult.Item1;
                                _logger.LogDebug($"Updated {found} of {totalCount} nodes from automate.");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug($"Updated {found} of {obj.Length} nodes from automate.");
                    }
                }
            }
            return list.ToArray();
        }

        private async Task<Tuple<int,ComplianceNode[]>> SaveAndReturnFailuresAndFound(IEnumerable<ComplianceNode> array)
        {
            
            var list = new List<ComplianceNode>();
            var found = 0;
            foreach (var obj in array)
            {
                var endtime = (obj.latest_report?.end_time).GetValueOrDefault();
                var id = (obj.latest_report?.id).GetValueOrDefault();
                var node = await _ctx.Nodes.FirstOrDefaultAsync(p =>
                    p.Fqdn == obj.name && 
                    (p.LastComplianceResultId == null || p.LastComplianceResultId != id) && 
                    (p.LastComplianceResultDate == null || p.LastComplianceResultDate < endtime));
                if (node != null)
                {
                    found += 1;
                    node.ChefNodeId = obj.id;
                    node.ComplianceStatus = (obj.latest_report?.GetComplainceStatus()).GetValueOrDefault();
                    node.LastComplianceResultDate = obj.latest_report?.end_time;
                    node.LastComplianceResultId = obj.latest_report?.id;
                    if (obj.latest_report != null)
                    {
                        if (node.ComplianceStatus == ComplianceStatusConstant.Succeeded)
                        {
                            node.FailingSince = null;
                            node.LastEmailedOn = null;
                            if (!_ctx.ComplianceResults.Any(p =>
                                p.InventoryItemId == node.InventoryItemId && 
                                p.ResultId == obj.latest_report.id))
                            {
                                //for successes enter a record into results table, for graphs,
                                //for failures we will collect details and add at that time
                                _ctx.ComplianceResults.Add(new ComplianceResult
                                {
                                    EndDate = obj.latest_report.end_time.Date,
                                    EndTime = obj.latest_report.end_time,
                                    InventoryItemId = node.InventoryItemId,
                                    Status = ComplianceStatusConstant.Succeeded,
                                    ResultId = obj.latest_report.id,
                                    OperatingSystemTestPassed = true
                                });
                            }
                        }
                        else if(node.ComplianceStatus == ComplianceStatusConstant.Failed)
                        {
                            list.Add(obj);
                        }
                    }

                    await _ctx.SaveChangesAsync();
                }
            }

            return Tuple.Create(found, list.ToArray());
        }

        public async Task<int> PurgeOldComplianceRuns()
        {
            var date = DateTime.Today.AddDays(-28);
            var count = 0;
            _logger.LogDebug($"Gathering Old Compliance runs to purge prior to {date.ToShortDateString()}");
            var maxQ = _ctx.ComplianceResults.Where(p => p.EndDate < date).Select(p => p.Id);
            
            var max = (await maxQ.AnyAsync()) ? (await maxQ.MaxAsync()) : -1;
            
            _logger.LogDebug("Purging Old Compliance Runs");
            if (max > 0)
            {
                count += await _ctx.ExecuteCommandAsync(
                    "DELETE r FROM ComplianceResult r where Id < @max",
                    new MySqlParameter("@max", MySqlDbType.Int64){Value=max});
            }
            _logger.LogDebug("Purging Compliance Runs from inactive nodes");

            count += await _ctx.ExecuteCommandAsync("DELETE r FROM ComplianceResult r JOIN Node n ON r.InventoryItemId = n.InventoryItemId WHERE n.IsActive = 0");
            
            return count;
        }
        
        public async Task<int> PurgeOldComplianceDetails()
        {
            var count = await _ctx.ComplianceResultTests.CountAsync();
            count += await _ctx.ComplianceResultErrors.CountAsync();
            await _ctx.ExecuteCommandAsync("TRUNCATE TABLE ComplianceResultTest");
            await _ctx.ExecuteCommandAsync("TRUNCATE TABLE ComplianceResultError");
            return count;
        }

        public async Task<int> ResetComplianceStatus()
        {
            _logger.LogDebug("Gathering Nodes with Stale Compliance Status");
            var list = await _ctx.Nodes.WithStaleComplianceStatus().ToArrayAsync();
            if (list.Length > 0)
            {
                _logger.LogDebug($"Resetting Compliance Status on  {list.Length} Nodes");
                foreach (var node in list)
                {
                    node.ComplianceStatus = ComplianceStatusConstant.NotFound;
                    node.LastComplianceResultDate = null;
                    node.FailingSince = null;
                    node.LastEmailedOn = null;
                    await _ctx.SaveChangesAsync();
                    _logger.LogInformation($"Reset Compliance status on Node {node.Fqdn}.");
                 
                }
            }

            return list.Length;
        }
        private string BuildNodesUrl(int perPage, int page, Guid[] nodes)
        {
            var builder = new StringBuilder();
            builder.Append("/compliance/nodes");
            string nodeFilter = string.Empty;
            if (nodes != null &&  nodes.Length <= 128)
            {
                nodeFilter = $"+node_id:{string.Join("+node_id:", nodes)}";
            }
            var keys = new Dictionary<string, string>
            {
                {"filters", $"end_time:{HttpUtility.UrlEncode(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"))}{nodeFilter}"},
                {"per_page", perPage.ToString()},
                {"page", page.ToString()}
            };
            builder.Append(ToQueryString(keys));
            return builder.ToString();
        }

        private string BuildReportsUrl(Guid id)
        {
            var builder = new StringBuilder();
            builder.Append("/compliance/reports/");
            builder.Append(id);
            return builder.ToString();
        }

        private static string ToQueryString(IDictionary<string, string> nvc)
        {
            return "?" + string.Join("&", nvc.Select(p => $"{p.Key}={p.Value}"));
        }
        
        
        public async Task<int> PurgeInactiveNodes()
        {
            var date = DateTime.Today.AddDays(-7);
            var count = 0;
            _logger.LogDebug($"Getting Nodes Deactivated prior to {date.ToShortDateString()}");
            while (await _ctx.Nodes.Inactive().AnyAsync(p => p.DeactivatedDate < date))
            {
                _logger.LogDebug("Purging 1000 Deactivated Nodes");
                await _ctx.ExecuteCommandAsync(
                    "DELETE FROM Node WHERE DeactivatedDate < @date or IsActive = 0 ORDER BY InventoryItemId limit 1000",
                    new MySqlParameter("@date", MySqlDbType.Date) {Value = date});
                

                count += 1000;
            }

            return count;
        }
        
        public async Task<int> NotifyUnassignedNodes()
        {
            var nodes = await _ctx.Nodes.AsNoTracking().Active().InPciScope().Unassigned().ProductIsNotExlcuded()
                .Select(p => new {p.Owner, p.Fqdn, p.PciScope})
                .ToArrayAsync();

            if (nodes.Length > 0)
            {
                _logger.LogDebug($"Emailing {nodes.Length} Unassigned Nodes");

                foreach (var node in nodes)
                {
                    var name = node.Owner.OwnerText(false);
                    var email = node.Owner.Email;
                    _logger.LogInformation(
                        $"Emailing {name} at email {email} about {node.Fqdn} with pci-scope {node.PciScope}");
                    await SendUnassignedMail(email, name, node.Fqdn, node.PciScope.ToString());
                }
            }

            return nodes.Length;
        }
        public async Task<int> NotifyNotReportingNodes()
        {
            var nodes = await _ctx.Nodes.AsNoTracking().Active().InChefScope().InPciScope().Assigned().ProductIsNotExlcuded().ByComplianceStatus(ComplianceStatusConstant.NotFound)
                .Select(p => new {p.Owner, p.Fqdn, p.PciScope, AppOwnerEmail = p.BuildSpecification.Owner.Email, OsOwnerEmail = p.BuildSpecification.Parent.Owner.Email})
                .ToArrayAsync();

            if (nodes.Length > 0)
            {
                _logger.LogDebug($"Emailing {nodes.Length} Not Reporting Nodes");

                foreach (var node in nodes)
                {
                    var name = node.Owner.OwnerText(false);
                    var emails = new []{node.Owner.Email, node.AppOwnerEmail, node.OsOwnerEmail};
                    _logger.LogInformation(
                        $"Emailing {name} at email {node.Owner.Email} and {node.AppOwnerEmail} and {node.OsOwnerEmail} about {node.Fqdn} with pci-scope {node.PciScope}");
                    await SendNotReportingMail(emails, name, node.Fqdn, node.PciScope.ToString());
                }
            }

            return nodes.Length;
        }

        private async Task SendUnassignedMail(string email, string name, string fqdn, string pci)
        {
          
            var builder = new StringBuilder()
                .Append(
                    $"<p>{name}, you are receiving this email because you are the identified owner of {fqdn} and this server is not assigned to an Application specification within ACT.  Please assign this server to an Application Specification to resolve this email alert.</p>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");
            
            await SendMail(new []{email}, $"ACT Unassigned Failure for a PCI '{pci}' class system - {fqdn}",
                builder.ToString());
        }
        
        private async Task SendNotReportingMail(string[] emails, string name, string fqdn, string pci)
        {
     
            var builder = new StringBuilder()
                .Append(
                    $"<p>{name}, you are receiving this email because you are the identified owner of {fqdn} or its Application or OS Specification and this server has not reported to chef within 48 hours.  Please ensure the node is still running the chef-client to resolve this email alert.</p>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");

            await SendMail(emails, $"ACT Not Reporting Failure for a PCI '{pci}' class system - {fqdn}",
                builder.ToString());
        }
        
        public async Task SendMail(string[] emails, string subject, string body)
        {
            if (emails == null || emails.Length == 0)
                throw new ArgumentNullException(nameof(emails));

            using (var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port))
            {
                smtp.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
                smtp.UseDefaultCredentials = true;

                var mm = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    From = new MailAddress(_mailSettings.From)
                };
                foreach (var email in emails) 
                    mm.To.Add(email);
                
                await smtp.SendMailAsync(mm);
            }
        }
    }
}

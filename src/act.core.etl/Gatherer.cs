using act.core.data;
using act.core.etl.ComplianceModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Environment = act.core.data.Environment;
using Pomelo.EntityFrameworkCore.MySql;
using MySqlConnector;
namespace act.core.etl
{
    public class Gatherer : IGatherer
    {

        private readonly ActDbContext _ctx;
        private readonly ILogger _logger;
        private readonly MailSettings _mailSettings;

        public Gatherer(ActDbContext ctx, ILoggerFactory logger, IConfiguration configuration)
        {
            _ctx = ctx;
            _mailSettings = new MailSettings();
            configuration.GetSection("Mail").Bind(_mailSettings);
            var forcedTest = System.Environment.GetEnvironmentVariable("ACT_TEST_MAIL");
            if (!string.IsNullOrWhiteSpace(forcedTest))
                _mailSettings.TestRecipient = forcedTest.Trim();
            _logger = logger.CreateLogger<Gatherer>();
        }

        public async Task<HttpResponseMessage> Proxy(int environmentId, string url)
        {
            using (var client = new HttpClient())
            {
                SetHeaders(client, await _ctx.Environments.ById(environmentId));
                return await client.GetAsync(url);
            }
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

        private static readonly HashSet<string> PciProfiles = new HashSet<string> { "csg_windows_compliant_server", "csg_linux_compliant_server" };
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
                if (status == ComplianceStatusConstant.Failed)
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
            var results = profile.controls.Where(p => ShouldNotControls.Contains(p.id)).Where(p => p.results != null).SelectMany(p => p.results);
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
                            LongMessage = result.message,
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
            message.end_time_utc = message.failed_critical_profiles?[0].controls?[0].results?[0].start_time;
            var node = await _ctx.Nodes.Active().ById(message.node_uuid);
            var text = node == null ? "did not find" : "found";
            _logger.LogInformation($"NotifyComplianceFailure WebHook got a compliance failure message for chef node id {message.node_uuid} and {text} a matching node {node?.Fqdn}.");
            if (node != null)
            {
                var run = await SaveComplianceData(node.InventoryItemId, message);

                if (run != null)
                {
                    if (run.EndDate != DateTime.MinValue)
                    {
                        node.LastComplianceResultDate = run.EndTime;
                        node.FailingSince ??= run.EndTime;
                    }
                    node.LastComplianceResultId = run.ResultId;

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
                !(await _ctx.ComplianceResults.Exists(node.InventoryItemId, node.LastComplianceResultId.Value)))
            {
                var rpt = await GetReport(node.EnvironmentId, node.LastComplianceResultId.Value);

                var result = await SaveComplianceData(node.InventoryItemId, rpt);
                node.ComplianceStatus = result.Status;
                await _ctx.SaveChangesAsync();
            }

        }

        private async Task<ComplianceReport> GetReport(int environmentId, Guid latestReportId)
        {
            return await PostRequest<ComplianceReport>(environmentId, BuildReportsUrl(latestReportId));

        }
        public async Task<ComplianceNode[]> Gather(int environmentId, string[] fqdns = null)
        {

            var list = new List<ComplianceNode>();

            var complianceNodes = await PostRequest(environmentId, 100, 1, fqdns);
            if (complianceNodes != null)
            {
                var saveResult = await SaveAndReturnFailuresAndFound(complianceNodes.Nodes);
                list.AddRange(saveResult.Item2);
                var found = saveResult.Item1;

                if (complianceNodes.Total > 0)
                {
                    _logger.LogInformation($"Updated {found} of {complianceNodes.Total} nodes from automate.");
                    var pageCount = complianceNodes.Total / 100 + ((complianceNodes.Total % 100) > 0 ? 1 : 0);
                    for (var p = 2; p <= pageCount; p++)
                    {
                        complianceNodes = await PostRequest(environmentId, 100, p, fqdns);
                        if (complianceNodes != null)
                        {
                            saveResult = await SaveAndReturnFailuresAndFound(complianceNodes.Nodes);
                            list.AddRange(saveResult.Item2);
                            found += saveResult.Item1;
                            _logger.LogInformation($"Updated {found} of {complianceNodes.Total} nodes from automate.");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"Updated {found} of {complianceNodes.Total} nodes from automate.");
                }
            }
            return list.ToArray();
        }

        public async Task<ComplianceNodes> PostRequest(int environmentId, int perPage = 100, int page = 1, string[] fqdns = null)
        {
            var json = JsonConvert.SerializeObject(BuildNodesUrl(perPage, page, fqdns));
            return await PostRequest<ComplianceNodes>(environmentId, "/api/v0/nodes/search", json);

        }


        private async Task<T> PostRequest<T>(int environmentId, string url, string json = null)
        {
            using (var client = new HttpClient())
            {
                var environment = await _ctx.Environments.ById(environmentId);
                client.BaseAddress = new Uri(environment.ChefAutomateUrl);
                client.DefaultRequestHeaders.Add("api-token", environment.ChefAutomateToken);
                HttpResponseMessage responseMessage;
                if (json != null)
                {
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    responseMessage = await client.PostAsync(url, httpContent);
                }
                else
                {
                    responseMessage = await client.PostAsync(url, null);
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    var content = await responseMessage.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }

            return default(T);

        }

        private async Task<Tuple<int, ComplianceNode[]>> SaveAndReturnFailuresAndFound(IEnumerable<ComplianceNode> nodes)
        {

            var list = new List<ComplianceNode>();
            var found = 0;
            foreach (var node in nodes)
            {
                var endTime = (node.ScanData?.end_time).GetValueOrDefault();
                var id = (node.ScanData?.id).GetValueOrDefault();
                var nodeInfo = await _ctx.Nodes.FirstOrDefaultAsync(p =>
                    p.Fqdn == node.Name &&
                    (p.LastComplianceResultId == null || p.LastComplianceResultId != id) &&
                    (p.LastComplianceResultDate == null || p.LastComplianceResultDate < endTime));
                if (nodeInfo != null && id != Guid.Empty)
                {
                    found += 1;
                    nodeInfo.ChefNodeId = node.Id;
                    nodeInfo.ComplianceStatus = (node.ScanData?.GetComplainceStatus()).GetValueOrDefault();
                    nodeInfo.LastComplianceResultDate = node.ScanData?.end_time;
                    nodeInfo.LastComplianceResultId = node.ScanData?.id;
                    if (node.ScanData != null)
                    {
                        if (nodeInfo.ComplianceStatus == ComplianceStatusConstant.Succeeded)
                        {
                            nodeInfo.FailingSince = null;
                            nodeInfo.LastEmailedOn = null;
                            nodeInfo.EscalationMailedOn = null;
                            nodeInfo.EscalationStep = 0;
                            if (!_ctx.ComplianceResults.Any(p =>
                                p.InventoryItemId == nodeInfo.InventoryItemId &&
                                p.ResultId == node.ScanData.id))
                            {
                                //for successes enter a record into results table, for graphs,
                                //for failures we will collect details and add at that time
                                _ctx.ComplianceResults.Add(new ComplianceResult
                                {
                                    EndDate = (node.ScanData.end_time ?? endTime).Date,
                                    EndTime = (node.ScanData.end_time ?? endTime),
                                    InventoryItemId = nodeInfo.InventoryItemId,
                                    Status = ComplianceStatusConstant.Succeeded,
                                    ResultId = node.ScanData.id.Value,
                                    OperatingSystemTestPassed = true
                                });
                            }
                        }
                        else if (nodeInfo.ComplianceStatus == ComplianceStatusConstant.Failed)
                        {
                            nodeInfo.FailingSince ??= node.ScanData.end_time ?? DateTime.Now;
                            list.Add(node);
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
            _logger.LogInformation($"Gathering Old Compliance runs to purge prior to {date.ToShortDateString()}");
            var maxQ = _ctx.ComplianceResults.Where(p => p.EndDate < date).Select(p => p.Id);

            var max = (await maxQ.AnyAsync()) ? (await maxQ.MaxAsync()) : -1;

            _logger.LogInformation("Purging Old Compliance Runs");
            if (max > 0)
            {
                count += await _ctx.ExecuteCommandAsync(
                    "DELETE r FROM ComplianceResult r where Id < @max",
                    new MySqlParameter("@max", MySqlDbType.Int64) { Value = max });
            }
            _logger.LogInformation("Purging Compliance Runs from inactive nodes");

            count += await _ctx.ExecuteCommandAsync("DELETE r FROM ComplianceResult r JOIN Node n ON r.InventoryItemId = n.InventoryItemId WHERE n.IsActive = 0");

            return count;
        }

        public async Task<int> PurgeOldComplianceDetails()
        {
            await _ctx.ExecuteCommandAsync("TRUNCATE TABLE ComplianceResultTest");
            await _ctx.ExecuteCommandAsync("TRUNCATE TABLE ComplianceResultError");
            return 0;
        }

        public async Task<int> ResetComplianceStatus()
        {
            _logger.LogInformation("Gathering Nodes with Stale Compliance Status");
            var list = await _ctx.Nodes.WithStaleComplianceStatus().ToArrayAsync();
            if (list.Length > 0)
            {
                _logger.LogInformation($"Resetting Compliance Status on  {list.Length} Nodes");
                foreach (var node in list)
                {
                    node.ComplianceStatus = ComplianceStatusConstant.NotFound;
                    node.FailingSince = null;
                    node.LastEmailedOn = null;
                    node.EscalationMailedOn = null;
                    node.EscalationStep = 0;
                    await _ctx.SaveChangesAsync();
                    _logger.LogInformation($"Reset Compliance status on Node {node.Fqdn}.");

                }
            }

            return list.Length;
        }

        public async Task<int> ResetPciClassCEscalation()
        {
            var count = await _ctx.ExecuteCommandAsync(
                "UPDATE Node SET EscalationMailedOn = NULL, EscalationStep = 0 WHERE EscalationMailedOn IS NOT NULL OR EscalationStep <> 0");
            _logger.LogInformation($"Cleared PCI Class C escalation stamps on {count} node(s).");
            return count;
        }
        private JObject BuildNodesUrl(int perPage, int page, string[] fqdns)
        {
            var bodyContent = new JObject
            {
                ["per_page"] = perPage,
                ["page"] = page
            };

            if (fqdns != null && fqdns.Length > 0 && fqdns.Length <= 128)
            {
                for (var i = 0; i < fqdns.Length; i++)
                {
                    fqdns[i] = fqdns[i].Replace("com", "*").Replace("COM", "*");

                }
                var filter = new JObject
                    {
                        { "key","name"},
                        {"values", new JArray(fqdns) }
                    };
                bodyContent["filters"] = new JArray(filter);
            };
            return bodyContent;
        }

        private string BuildReportsUrl(Guid id)
        {
            var builder = new StringBuilder();
            builder.Append("/api/v0/compliance/reporting/reports/id/");
            builder.Append(id);
            return builder.ToString();
        }

        public async Task<int> PurgeInactiveNodes()
        {
            var deactivationCutoff = DateTime.Today.AddDays(-7);
            var complianceCutoff = DateTime.Today.AddDays(-15);
            var count = 0;

            _logger.LogInformation($"Getting Nodes Deactivated prior to {deactivationCutoff.ToShortDateString()}");
            var deactivatedNodes = await _ctx.Nodes.Inactive().Where(p => p.DeactivatedDate < deactivationCutoff)
                .Select(p => new { p.Fqdn, p.DeactivatedDate, p.BuildSpecificationId })
                .ToArrayAsync();

            foreach (var node in deactivatedNodes)
            {
                _logger.LogInformation($"Deactivated Node: fqdn {node.Fqdn}, deactivated date: {node.DeactivatedDate}, buildspecid: {node.BuildSpecificationId}");
            }

            _logger.LogInformation($"Getting Nodes with LastComplianceResultDate older than {complianceCutoff.ToShortDateString()}");
            var staleComplianceNodes = await _ctx.Nodes
                .Where(p => p.LastComplianceResultDate < complianceCutoff)
                .Select(p => new { p.Fqdn, p.LastComplianceResultDate, p.BuildSpecificationId })
                .ToArrayAsync();

            foreach (var node in staleComplianceNodes)
            {
                _logger.LogInformation($"Stale Compliance Node: fqdn {node.Fqdn}, lastComplianceResultDate: {node.LastComplianceResultDate}, buildspecid: {node.BuildSpecificationId}");
            }

            while (
                await _ctx.Nodes.Inactive().AnyAsync(p => p.DeactivatedDate < deactivationCutoff)
                || await _ctx.Nodes.AnyAsync(p => p.LastComplianceResultDate < complianceCutoff))
            {
                _logger.LogInformation("Purging up to 1000 Inactive or Stale-Compliance Nodes");
                var deleted = await _ctx.ExecuteCommandAsync(
                    "DELETE FROM Node " +
                    "WHERE (IsActive = 0 AND DeactivatedDate < @deactivationDate) " +
                    "   OR LastComplianceResultDate < @complianceDate " +
                    "ORDER BY InventoryItemId LIMIT 1000",
                    new MySqlParameter("@deactivationDate", MySqlDbType.Date) { Value = deactivationCutoff },
                    new MySqlParameter("@complianceDate", MySqlDbType.Date) { Value = complianceCutoff });

                count += deleted;
            }

            return count;
        }

        public async Task<int> DeactivateNode(int id)
        {
            _logger.LogInformation($"Deactivating node with id {id}");
            var node = await _ctx.Nodes.ById(id);
            if (node != null)
            {
                node.DeactivatedDate = DateTime.Now;
                node.IsActive = false;
                await _ctx.SaveChangesAsync();
                return 1;
            }

            return 0;
        }

        public async Task<int> NotifyUnassignedNodes()
        {
            var nodes = await _ctx.Nodes.AsNoTracking().Active().InPciScope().Unassigned().ProductIsNotExlcuded()
                .Select(p => new { p.Owner, p.Fqdn, p.PciScope, p.RemedyGroupEmailList })
                .ToArrayAsync();

            if (nodes.Length > 0)
            {
                _logger.LogInformation($"Emailing {nodes.Length} Unassigned Nodes");

                foreach (var node in nodes)
                {
                    var name = node.Owner.OwnerText(false);
                    var email = new[] {node.Owner.Email, node.RemedyGroupEmailList};

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

                .Select(p => new { p.Owner, p.Fqdn, p.PciScope, AppOwnerEmail = p.BuildSpecification.Owner.Email, OsOwnerEmail = p.BuildSpecification.Parent.Owner.Email, LastComplianceDate = p.LastComplianceResultDate, p.RemedyGroupEmailList })

                .ToArrayAsync();

            if (nodes.Length > 0)
            {
                _logger.LogInformation($"Emailing {nodes.Length} Not Reporting Nodes");

                foreach (var node in nodes)
                {
                    var name = node.Owner.OwnerText(false);

                    var emails = new[] { node.Owner.Email, node.AppOwnerEmail, node.OsOwnerEmail, node.RemedyGroupEmailList };

                    _logger.LogInformation(
                        $"Emailing {name} at email {node.Owner.Email} and {node.AppOwnerEmail} and {node.OsOwnerEmail} about {node.Fqdn} with pci-scope {node.PciScope}");
                    await SendNotReportingMail(emails, name, node.Fqdn, node.PciScope.ToString(), node.LastComplianceDate);
                }
            }

            return nodes.Length;
        }

        public async Task<int> NotifyPciClassCNodes()
        {
            var now = DateTime.Now;
            var dueAfter = now.AddDays(-7);

            var chefNodes = await LoadChefInventoryStillOnPciPolicy();
            if (chefNodes.Count == 0)
            {
                _logger.LogWarning(
                    "Chef inventory returned no PCI-policy nodes; sending no PCI Class C digests.");
                return 0;
            }

            var actNodes = await _ctx.Nodes.AsNoTracking()
                .Active()
                .InChefScope()
                .ByPciScope(PciScopeConstant.C)
                .ProductIsNotExlcuded()
                .Select(p => new
                {
                    p.InventoryItemId,
                    p.OwnerEmployeeId,
                    p.Fqdn,
                    p.EscalationMailedOn,
                    p.EscalationStep
                })
                .ToArrayAsync();

            var stillOnChef = new List<(long InventoryItemId, long OwnerEmployeeId, string Fqdn, DateTime? EscalationMailedOn, int EscalationStep, CfgmgmtNode Chef)>();
            foreach (var node in actNodes)
            {
                if (string.IsNullOrWhiteSpace(node.Fqdn))
                    continue;
                if (!TryFindChefNode(chefNodes, node.Fqdn, out var chef))
                    continue;
                stillOnChef.Add((node.InventoryItemId, node.OwnerEmployeeId, node.Fqdn, node.EscalationMailedOn, node.EscalationStep, chef));
            }

            var testDelivery = !string.IsNullOrWhiteSpace(_mailSettings.TestRecipient);
            var due = stillOnChef
                .Select(p => new
                {
                    Node = p,
                    Levels = testDelivery ? 0 : EscalationLevels(p.EscalationMailedOn, p.EscalationStep)
                })
                .Where(p => testDelivery || IsEscalationDue(p.Node.EscalationMailedOn, dueAfter))
                .ToArray();

            _logger.LogInformation(
                $"Chef PCI-policy inventory {chefNodes.Count} nodes; ACT Class C active {actNodes.Length}; still reporting to Chef {stillOnChef.Count}; due this week {due.Length}.");

            if (stillOnChef.Count == 0)
            {
                _logger.LogWarning(
                    $"No ACT Class C FQDN matched Chef PCI inventory. Sample Chef keys: {string.Join(",", chefNodes.Keys.Take(15))}.");
                return 0;
            }

            if (due.Length == 0)
                return 0;

            var employees = await _ctx.Employees.LoadWithSupervisorChain(due.Select(p => p.Node.OwnerEmployeeId));
            var groups = due.GroupBy(p => new { p.Node.OwnerEmployeeId, p.Levels }).ToArray();
            _logger.LogInformation(
                $"Emailing {groups.Length} PCI Class C Chef-removal digests covering {due.Length} nodes");

            foreach (var group in groups)
            {
                employees.TryGetValue(group.Key.OwnerEmployeeId, out var owner);
                if (owner == null)
                {
                    _logger.LogWarning(
                        $"Skipping PCI Class C digest; no Employee row for owner id {group.Key.OwnerEmployeeId}.");
                    continue;
                }

                var name = owner.OwnerText(false);
                var names = new List<string>();
                var intendedEmails = new List<string>();

                owner.AddMailRecipient(intendedEmails, names);
                owner.AddEscalationLevels(intendedEmails, names, group.Key.Levels);

                var toNames = names.Distinct().Reverse().ToArray();
                var intended = intendedEmails.Distinct().ToArray();
                var toSend = ResolvePciClassCRecipients(intended);
                if (toSend.Length == 0)
                {
                    _logger.LogWarning(
                        $"Skipping PCI Class C digest for owner id {group.Key.OwnerEmployeeId}; no recipient email.");
                    continue;
                }

                var cc = ResolvePciClassCCc();

                _logger.LogInformation(
                    $"Emailing PCI Class C digest for {name} with {group.Count()} server(s), escalation levels {group.Key.Levels}, to {string.Join(",", toSend)} cc {string.Join(",", cc)} (intended {string.Join(",", intended)})");

                await SendPciClassCDigestMail(toSend, cc, toNames, name, group.Select(n =>
                    (n.Node.Fqdn, n.Node.Chef.ChefVersion, n.Node.Chef.PolicyName, n.Node.Chef.PolicyGroup,
                        ServiceLevelFromPolicyGroup(n.Node.Chef.PolicyGroup))).ToArray());

                if (testDelivery)
                    continue;

                var ids = group.Select(n => n.Node.InventoryItemId).ToArray();
                var toStamp = await _ctx.Nodes.Where(p => ids.Contains(p.InventoryItemId)).ToArrayAsync();
                foreach (var node in toStamp)
                {
                    node.EscalationMailedOn = now;
                    node.EscalationStep = group.Key.Levels + 1;
                }
                await _ctx.SaveChangesAsync();
            }

            return groups.Length;
        }

        private static bool IsEscalationDue(DateTime? escalationMailedOn, DateTime dueAfter)
        {
            if (!escalationMailedOn.HasValue)
                return true;

            return escalationMailedOn.Value <= dueAfter;
        }

        private static int EscalationLevels(DateTime? escalationMailedOn, int escalationStep)
        {
            if (!escalationMailedOn.HasValue)
                return 0;

            return escalationStep < 0 ? 0 : escalationStep;
        }

        private async Task<Dictionary<string, CfgmgmtNode>> LoadChefInventoryStillOnPciPolicy()
        {
            var map = new Dictionary<string, CfgmgmtNode>(StringComparer.OrdinalIgnoreCase);
            var environmentIds = await _ctx.Environments.AsNoTracking().Select(e => e.Id).ToArrayAsync();
            foreach (var environmentId in environmentIds)
            {
                try
                {
                    var page = 1;
                    const int pageSize = 1000;
                    while (page <= 50)
                    {
                        var url = $"/api/v0/cfgmgmt/nodes?pagination.page={page}&pagination.size={pageSize}";
                        var batch = await GetChefInventoryPage(environmentId, url);
                        if (batch.Length == 0)
                            break;

                        foreach (var node in batch)
                        {
                            if (!IsPciChefPolicy(node))
                                continue;
                            AddChefInventoryKeys(map, node);
                        }

                        if (batch.Length < pageSize)
                            break;
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Chef inventory refresh failed for environment {environmentId}.");
                }
            }

            return map;
        }

        private async Task<CfgmgmtNode[]> GetChefInventoryPage(int environmentId, string url)
        {
            var content = await GetRequestContent(environmentId, url);
            if (string.IsNullOrWhiteSpace(content))
                return Array.Empty<CfgmgmtNode>();

            var trimmed = content.TrimStart();
            if (trimmed.StartsWith("["))
                return JsonConvert.DeserializeObject<CfgmgmtNode[]>(content) ?? Array.Empty<CfgmgmtNode>();

            var wrapped = JsonConvert.DeserializeObject<CfgmgmtNodes>(content);
            return wrapped?.Nodes ?? Array.Empty<CfgmgmtNode>();
        }

        private async Task<string> GetRequestContent(int environmentId, string url)
        {
            using (var client = new HttpClient())
            {
                var environment = await _ctx.Environments.ById(environmentId);
                if (environment == null || string.IsNullOrWhiteSpace(environment.ChefAutomateUrl))
                    return null;

                client.BaseAddress = new Uri(environment.ChefAutomateUrl);
                client.DefaultRequestHeaders.Add("api-token", environment.ChefAutomateToken);
                var responseMessage = await client.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        $"Chef GET {url} for environment {environmentId} returned {(int)responseMessage.StatusCode}.");
                    return null;
                }

                return await responseMessage.Content.ReadAsStringAsync();
            }
        }

        private static bool IsPciChefPolicy(CfgmgmtNode node)
        {
            if (node == null)
                return false;

            var policy = (node.PolicyName ?? string.Empty).Trim();
            var group = (node.PolicyGroup ?? string.Empty).Trim();
            return policy.StartsWith("pci", StringComparison.OrdinalIgnoreCase) ||
                   group.StartsWith("pci", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddChefInventoryKeys(Dictionary<string, CfgmgmtNode> map, CfgmgmtNode node)
        {
            if (node == null)
                return;

            AddChefInventoryKey(map, node.Fqdn, node);
            AddChefInventoryKey(map, node.Name, node);
        }

        private static void AddChefInventoryKey(Dictionary<string, CfgmgmtNode> map, string key, CfgmgmtNode node)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var trimmed = key.Trim();
            map[trimmed] = node;
            var dot = trimmed.IndexOf('.');
            if (dot > 0)
                map[trimmed.Substring(0, dot)] = node;
        }

        private static bool TryFindChefNode(Dictionary<string, CfgmgmtNode> chefNodes, string fqdn, out CfgmgmtNode chef)
        {
            if (chefNodes.TryGetValue(fqdn.Trim(), out chef))
                return true;

            var host = fqdn.Trim();
            var dot = host.IndexOf('.');
            if (dot > 0 && chefNodes.TryGetValue(host.Substring(0, dot), out chef))
                return true;

            chef = null;
            return false;
        }

        private static string ServiceLevelFromPolicyGroup(string policyGroup)
        {
            var group = policyGroup ?? string.Empty;
            if (group.IndexOf("cte", StringComparison.OrdinalIgnoreCase) >= 0)
                return "CTE";
            if (group.IndexOf("prod", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Production";
            return "Non-Prod";
        }

        private string[] ResolvePciClassCRecipients(string[] intendedEmails)
        {
            if (!string.IsNullOrWhiteSpace(_mailSettings.TestRecipient))
                return new[] { _mailSettings.TestRecipient };

            return intendedEmails ?? Array.Empty<string>();
        }

        private string[] ResolvePciClassCCc()
        {
            if (string.IsNullOrWhiteSpace(_mailSettings.EscalationCc))
                return Array.Empty<string>();

            return new[] { _mailSettings.EscalationCc.Trim() };
        }

        private async Task SendPciClassCDigestMail(string[] emails, string[] cc, string[] names, string ownerName,
            (string Fqdn, string ClientVersion, string PolicyName, string PolicyGroup, string ServiceLevel)[] servers)
        {
            if (emails == null || emails.Length == 0)
            {
                _logger.LogWarning("Skipping PCI Class C digest; no recipient email.");
                return;
            }

            var greeting = names != null && names.Length > 0
                ? string.Join(", ", names)
                : "Team";

            var builder = new StringBuilder()
                .Append($"<p>Hello {greeting},</p>")
                .Append(
                    "<p>we are still seeing below servers reporting to Chef. Please create the <a href=\"https://csgsupport-dwp.onbmc.com/dwp/app/#/checkout\">WO</a> to remove the chef client on servers from pci policy. Use mySupport <a href=\"https://csgsupport-dwp.onbmc.com/dwp/app/#/checkout\">Submit a Compute General Request</a>.</p>")
                .Append($"<p>Total servers: <b>{servers.Length}</b></p>")
                .Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\" style=\"border-collapse:collapse\">")
                .Append("<tr><th>FQDN</th><th>Client Version</th><th>Policy Name</th><th>Policy Group</th><th>SecurityClass</th><th>ServiceLevel</th></tr>");

            foreach (var server in servers.OrderBy(s => s.Fqdn))
            {
                builder.Append(
                    $"<tr><td>{server.Fqdn}</td><td>{Cell(server.ClientVersion)}</td><td>{Cell(server.PolicyName)}</td><td>{Cell(server.PolicyGroup)}</td><td>C</td><td>{Cell(server.ServiceLevel)}</td></tr>");
            }

            builder
                .Append("</table>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");

            var subjectName = !string.IsNullOrWhiteSpace(ownerName) ? ownerName : "PCI Class C";
            await SendMail(emails,
                $"ACT PCI Class C Chef client still installed - {servers.Length} server(s) for {subjectName}",
                builder.ToString(),
                cc);
        }

        private static string Cell(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "&nbsp;" : value;
        }

        private async Task SendUnassignedMail(string[] emails, string name, string fqdn, string pci)
        {

            var builder = new StringBuilder()
                .Append(
                    $"<p>{name}, you are receiving this email because you are the identified owner of {fqdn} and this server is not assigned to an Application specification within ACT.  Please assign this server to an Application Specification to resolve this email alert.</p>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");

            await SendMail(emails, $"ACT Unassigned Failure for a PCI '{pci}' class system - {fqdn}",
                builder.ToString());
        }
        private async Task SendNotReportingMail(string[] emails, string name, string fqdn, string pci, DateTime? lastComplianceDate)
        {

            var builder = new StringBuilder()
                .Append(
                    $"<p>{name}, you are receiving this email because you are the identified owner of {fqdn} or its Application or OS Specification and this server has not reported to chef within 48 hours.  Please ensure the node is still running the chef-client to resolve this email alert.</p>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");

            if (lastComplianceDate.HasValue) builder.Append($"<br/><p><b>Note :</b> Last Compliance run date is {lastComplianceDate.Value.ToString("MM-dd-yyyy hh:mm tt")} </p>");

            await SendMail(emails, $"ACT Not Reporting Failure for a PCI '{pci}' class system - {fqdn}",
                builder.ToString());
        }

        public async Task SendMail(string[] emails, string subject, string body, string[] cc = null)
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
                    if (!string.IsNullOrEmpty(email))
                        mm.To.Add(email);
                if (cc != null)
                {
                    foreach (var email in cc)
                        if (!string.IsNullOrEmpty(email))
                            mm.CC.Add(email);
                }

                await smtp.SendMailAsync(mm);
            }
        }
    }
}


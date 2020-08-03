using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using act.core.data;
using act.core.etl;
using act.core.etl.ComplianceModel;
using act.core.web.Models.WebHook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace act.core.web.Services
{
    public interface INotifier
    {
        Task<NotifyComplianceFailureResult> NotifyComplianceFailure(string data);
    }

    internal class Notifier : INotifier
    {
        private readonly ActDbContext _ctx;
        private readonly IGatherer _gatherer;
        private readonly ILogger _logger;

        public Notifier(ActDbContext ctx, IGatherer gatherer, ILoggerFactory logger)
        {
            _logger = logger.CreateLogger<Notifier>();
            _ctx = ctx;
            _gatherer = gatherer;
        }

        public async Task<NotifyComplianceFailureResult> NotifyComplianceFailure(string data)
        {
            try
            {
                _logger.LogTrace($"NotifyComplianceFailure WebHook Called with data. {data}");
                var obj = JsonConvert.DeserializeObject<AutomateWebHookMessage>(data);
                _logger.LogTrace("NotifyComplianceFailure WebHook able to deserialize data.");

                if (obj != null)
                {
                    _logger.LogDebug(
                        $"recieved automate message via webhook of type {obj.type} with url {obj.automate_failure_url} and fqdn {obj.automate_fqdn} and chefid {obj.ChefNodeId}.");
                    await NotifyByChefId(obj);
                }

                return NotifyComplianceFailureResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception caught in NotifyComplianceFailureWebHook WebHook");
                return NotifyComplianceFailureResult.Failure(ex);
            }
        }

        private async Task SendNodeFailureMail(string[] emails, string name, string fqdn,
            AutomateWebHookMessage message)
        {
            var subject = $"Chef Converge Failure for fqdn: {fqdn} and node_name: {message.node_name}";
            var builder = new StringBuilder();
            builder
                .Append(
                    $"<p>{name}, you are receiving this email because you are the identified owner of {fqdn} or its Application Specification and this server has failed a Chef Converge.  The link to the Converge Review screen can be accessed via <a href=\"{message.automate_failure_url}\">{message.automate_failure_url}</a>.</p>")
                .Append("<p>Please take action to fix the converge failure.</p>")
                .Append("<h2>Failure Snippet</h2>")
                .Append($"<code>{message.failure_snippet}</code>")
                .Append("<h2>Exception</h2>")
                .Append($"<code>{message.exception_title}</code>")
                .Append("<h2>Message</h2>")
                .Append($"<code>{message.exception_message}</code>")
                .Append("<h2>Backtrace</h2>")
                .Append($"<code>{message.exception_backtrace}</code>")
                .Append("<p>Thank you,<br/>The CHEF Team</p>");
            await _gatherer.SendMail(emails, subject, builder.ToString());
        }

        private async Task SendComplianceFailureMail(string[] emails, string[] names, string fqdn, string pci, string link)
        {
            var subject = $"ACT Compliance Failure for a PCI '{pci}' class system - {fqdn}";

            var name = string.Join(", ", names);
            var allText = names.Length > 1 ? " all " : " ";
            var builder = new StringBuilder();
            builder
                .Append(
                    $"<p>{name}: you are{allText}receiving this email because you are the identified owner of {fqdn} or its Application or OS Specification and this server has failed the ACT Compliance checks.  The link to the Compliance Review screen can be accessed via <a href=\"{link}\">{link}</a>.</p>")
                .Append("<p>Please update your spec(s) or remediate the issue on the server.</p>")
                .Append("<p>Thank you,<br/>The Asset Compliance Tracker (ACT) Team</p>");

            await _gatherer.SendMail(emails, subject, builder.ToString());
        }
        
        private async Task NotifyByChefId(AutomateWebHookMessage message)
        {
            if (message.IsComplianceFailure)
            {
                await _gatherer.SaveComplianceDataFromWebHook(message);
                await NotifyComplianceByChefId(message);
            }
            else if (message.IsNodeFailure)
            {
                await NotifyNodeFailureByChefId(message);
            }
        }

        private async Task NotifyNodeFailureByChefId(AutomateWebHookMessage message)
        {
            var result = await _ctx.Nodes
                .AsNoTracking()
                .Where(p => p.ChefNodeId == message.ChefNodeId)
                .Active()
                .Select(p => new
                {
                    p.Fqdn,
                    p.Owner,
                    p.BuildSpecificationId
                }).FirstOrDefaultAsync();


            if (result != null)
            {
                var name = result.Owner.OwnerText(false);
                var emails = new List<string> {result.Owner.Email};
                if (result.BuildSpecificationId.HasValue)
                {
                    var bs = await _ctx.BuildSpecifications
                        .AsNoTracking()
                        .Include(p => p.Owner)
                        .ById(result.BuildSpecificationId.GetValueOrDefault());

                    emails.Add(bs.Owner.Email.ToLower());
                    if (!string.IsNullOrWhiteSpace(bs.EmailAddress)) emails.Add(bs.EmailAddress.ToLower());
                }

                var toSend = emails.Distinct().ToArray();
                _logger.LogInformation($"Emailing converge failure to {name} about {result.Fqdn}");

                await SendNodeFailureMail(toSend, name, result.Fqdn, message);
            }
            else
            {
                _logger.LogWarning(
                    $"recieved automate message via webhook of type {message.type} with url {message.automate_failure_url} and fqdn {message.automate_fqdn} and chefid {message.ChefNodeId} and node_name {message.node_name} but could not find the node in ACT.");
            }
        }

        private enum LevelConstants
        {
            Self,
            Boss,
            BossesBoss
        }
        
        private void AddToLists(Employee employee, LevelConstants level, ref List<string> emails, ref List<string> names)
        {

            Employee workOn = null;
            switch (level)
            {
                case LevelConstants.Self:
                    workOn = employee;
                    break;
                case LevelConstants.Boss:
                    workOn = employee?.Supervisor;
                    break;
                case LevelConstants.BossesBoss:
                    workOn = employee?.Supervisor?.Supervisor;
                    break;
            }

            if (workOn == null)
                return;

            var name = workOn.OwnerText(false);
            
            if(!string.IsNullOrWhiteSpace(name))
                names.Add(name);
            
            var email = workOn.Email?.ToLower();
            
            if(!string.IsNullOrWhiteSpace(email))
                emails.Add(email);
        }
        
        private async Task NotifyComplianceByChefId(AutomateWebHookMessage message)
        {
            var result = await _ctx.Nodes
                .Include(p=>p.Owner).ThenInclude(p=>p.Supervisor).ThenInclude(p=>p.Supervisor)
                .Active()
                .InPciScope()
                .InChefScope()
                .ProductIsNotExlcuded()
                .ById(message.ChefNodeId);


            var now = DateTime.Now;
            if (result?.BuildSpecificationId != null && (result.LastEmailedOn == null || result.LastEmailedOn < now.AddDays(-1)))
            {
                var names = new List<string>();
                var emails = new List<string>();

                AddToLists(result.Owner, LevelConstants.Self, ref emails, ref names);
                
                var bs = await _ctx.BuildSpecifications
                    .AsNoTracking()
                    .Include(p => p.Owner).ThenInclude(p=>p.Supervisor).ThenInclude(p=>p.Supervisor)
                    .Include(p => p.Parent).ThenInclude(p=>p.Owner).ThenInclude(p=>p.Supervisor).ThenInclude(p=>p.Supervisor)
                    .ById(result.BuildSpecificationId.GetValueOrDefault());

                
                if (!string.IsNullOrWhiteSpace(bs.EmailAddress)) 
                    emails.Add(bs.EmailAddress.ToLower()); //Add AppSpec Email

                if (!string.IsNullOrWhiteSpace(bs.Parent?.EmailAddress))
                    emails.Add(bs.Parent.EmailAddress.ToLower()); //Add OS Spec Email

                AddToLists(bs.Owner, LevelConstants.Self, ref emails, ref names);
                AddToLists(bs.Parent?.Owner, LevelConstants.Self, ref emails, ref names);               

                if (result.FailingSince < now.AddDays(-30))
                {
                    AddToLists(result.Owner, LevelConstants.Boss, ref emails, ref names);
                    AddToLists(bs.Owner, LevelConstants.Boss, ref emails, ref names);
                    AddToLists(bs.Parent?.Owner, LevelConstants.Boss, ref emails, ref names);
                }
                
                if (result.FailingSince < now.AddDays(-60))
                {
                    AddToLists(result.Owner, LevelConstants.BossesBoss, ref emails, ref names);
                    AddToLists(bs.Owner, LevelConstants.BossesBoss, ref emails, ref names);
                    AddToLists(bs.Parent?.Owner, LevelConstants.BossesBoss, ref emails, ref names);                   
                }

                var toSend = emails.Distinct().ToArray();
                var toNames = names.Distinct().Reverse().ToArray();
                _logger.LogInformation(
                    $"Emailing Compliance Failure to {string.Join(",", toNames)} about {result.Fqdn} with pci-scope {result.PciScope}");

                await SendComplianceFailureMail(toSend, toNames, result.Fqdn, result.PciScope.ToString(),
                    message.automate_failure_url);

                result.LastEmailedOn = now;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using act.core.etl.ComplianceModel;

namespace act.core.etl
{
    public interface IGatherer
    {
        Task<ComplianceNode[]> Gather(int environmentId, Guid[] nodeIds = null);
        Task ComplianceReport(long nodeId);
        Task<HttpResponseMessage> Proxy(int environmentId, string url);
        Task<int> PurgeOldComplianceRuns();
        Task<int> PurgeOldComplianceDetails();
        Task<int> ResetComplianceStatus();    
        Task SaveComplianceDataFromWebHook(AutomateWebHookMessage message);
        Task<int> NotifyUnassignedNodes();
        Task<int> NotifyNotReportingNodes();
        Task<int> PurgeInactiveNodes();
        Task SendMail(string[] emails, string subject, string body);
    }
}
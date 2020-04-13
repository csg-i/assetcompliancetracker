using System;
using System.Net.Http;
using System.Threading.Tasks;
using act.core.etl.ComplianceModel;

namespace act.core.etl
{
    public interface IGatherer
    {
        Task<ComplianceNode[]> Gather(int environmentId, string[] fqdns = null);
        Task ComplianceReport(long nodeId);
        Task<int> PurgeOldComplianceRuns();
        Task<int> PurgeOldComplianceDetails();
        Task<int> ResetComplianceStatus();    
        Task SaveComplianceDataFromWebHook(AutomateWebHookMessage message);
        Task<int> NotifyUnassignedNodes();
        Task<int> NotifyNotReportingNodes();
        Task<int> PurgeInactiveNodes();

        Task<int> DeactivateNode(int id);
        Task SendMail(string[] emails, string subject, string body);

        Task<ComplianceNodes> PostRequest(int environmentId, int perPage = 100, int page = 1,
            string[] fqdns = null);
    }
}
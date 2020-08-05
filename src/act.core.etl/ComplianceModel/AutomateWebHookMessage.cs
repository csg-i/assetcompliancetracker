using System;
using System.Linq;
using act.core.data;

namespace act.core.etl.ComplianceModel
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class AutomateWebHookMessage: IComplianceReport
    {
        public Guid ChefNodeId
        {
            get
            {
                if (IsComplianceFailure)
                {
                    return node_uuid;
                } 
                
                if (IsNodeFailure)
                {
                    var split = automate_failure_url
                                    .Replace($"https://{automate_fqdn}/infrastructure/client-runs/", string.Empty)
                                    .Replace("/runs/", "|")
                                    .Split("|")
                                    .FirstOrDefault() ?? string.Empty;
                    if (Guid.TryParse(split, out var g))
                        return g;
                }

                return Guid.Empty;
            }
        }

        DateTime IComplianceReport.end_time => Convert.ToDateTime(end_time_utc);
        Guid IComplianceReport.id { get; } = Guid.NewGuid();
        ComplianceReportProfile[] IComplianceReport.profiles => failed_critical_profiles;

        public string type { get; set; }
        public Guid node_uuid { get; set; }
        public string node_name { get; set; }
        public string automate_failure_url { get; set; }
        public string automate_fqdn { get; set; }
        public string failure_snippet { get; set; }
        public string exception_title { get; set; }
        public string exception_message { get; set; }
        public string exception_backtrace { get; set; }
        public string end_time_utc { get; set; }

        public ComplianceStatusConstant GetComplianceStatus()
        {
            return ComplianceStatusConstant.Failed;
        }

        public bool IsComplianceFailure => type == "compliance_failure";
        public bool IsNodeFailure => type == "converge_failure";

        public ComplianceReportProfile[] failed_critical_profiles { get; set; }
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
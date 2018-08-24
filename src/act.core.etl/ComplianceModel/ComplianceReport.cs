using System;
using act.core.data;

namespace act.core.etl.ComplianceModel
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public interface IComplianceReport
    {
        Guid id { get; }
        DateTime end_time { get; }
        ComplianceReportProfile[] profiles { get; }
        ComplianceStatusConstant GetComplianceStatus();
    }

    public class ComplianceReport : IComplianceReport
    {
        public Guid id { get; set; }

        // ReSharper disable MemberCanBePrivate.Global
        public string status { get; set; }
        // ReSharper restore MemberCanBePrivate.Global


        public DateTime end_time { get; set; }

        public ComplianceStatusConstant GetComplianceStatus()
        {
            return status == "failed" ? ComplianceStatusConstant.Failed : ComplianceStatusConstant.Succeeded;
        }

        public ComplianceReportProfile[] profiles { get; set; }
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
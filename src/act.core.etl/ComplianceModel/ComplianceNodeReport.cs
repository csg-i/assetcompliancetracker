using System;
using act.core.data;

namespace act.core.etl.ComplianceModel
{ 
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class ComplianceNodeReport
    {
        public DateTime? end_time { get; set; }
        public Guid? id { get; set; }
        public string status { get; set; }

        public ComplianceStatusConstant GetComplainceStatus()
        {
            if (status.ToLower() == "failed")
                return ComplianceStatusConstant.Failed;

            return ComplianceStatusConstant.Succeeded;
        }
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
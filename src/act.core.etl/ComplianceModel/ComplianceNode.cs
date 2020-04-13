using System;

namespace act.core.etl.ComplianceModel
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public class ComplianceNode
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public ComplianceNodeReport scan_data { get; set; }

    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    
}
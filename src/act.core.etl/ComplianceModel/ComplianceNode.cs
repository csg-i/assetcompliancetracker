using Newtonsoft.Json;
using System;

namespace act.core.etl.ComplianceModel
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public class ComplianceNode
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("scan_data")]
        public ComplianceNodeReport ScanData { get; set; }

    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    
}
using System.Collections.Generic;

namespace act.core.data
{
    public class SoftwareComponentEnvironment
    {
        
        public Environment Environment { get; set; }
        public SoftwareComponent SoftwareComponent { get; set; }
        public int EnvironmentId { get; set; }
        public long SoftwareComponentId { get; set; }
    }
}
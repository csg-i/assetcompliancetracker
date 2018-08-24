using System.Collections.Generic;

namespace act.core.data
{
    public class SoftwareComponent : JustificationTypeReference
    {
        public SoftwareComponent()
        {
            SoftwareComponentEnvironments = new HashSet<SoftwareComponentEnvironment>();
        }
        public override long Id { get; set; }

        public override long BuildSpecificationId { get; set; }

        public override JustificationTypeConstant JustificationType { get; set; }

        public string Name { get; set; }

        public bool NonCore { get; set; }

        public override byte[] TimeStamp { get; set; }

        public long? JustificationId { get; set; }

        public PciScopeConstant? PciScope { get; set; }

        public string Description { get; set; }

        public BuildSpecification BuildSpecification { get; set; }

        public Justification Justification { get; set; }
        
        public ICollection<SoftwareComponentEnvironment> SoftwareComponentEnvironments { get; set; }
    }
}
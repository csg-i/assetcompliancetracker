using System.Collections.Generic;

namespace act.core.data
{
    public class Justification : JustificationTypeReference
    {
        public Justification()
        {
            Ports = new HashSet<Port>();
            SoftwareComponents = new HashSet<SoftwareComponent>();
        }

        public override long Id { get; set; }

        public override long BuildSpecificationId { get; set; }

        public override JustificationTypeConstant JustificationType { get; set; }

        public string JustificationText { get; set; }

        public override byte[] TimeStamp { get; set; }

        public string Color { get; set; }

        public BuildSpecification BuildSpecification { get; set; }

        public ICollection<Port> Ports { get; set; }
        public ICollection<SoftwareComponent> SoftwareComponents { get; set; }
    }
}
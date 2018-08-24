using System.Collections.Generic;

namespace act.core.data
{
    public class BuildSpecification : LongId
    {
        public BuildSpecification()
        {
            Children = new HashSet<BuildSpecification>();
            Justifications = new HashSet<Justification>();
            Nodes = new HashSet<Node>();
            Ports = new HashSet<Port>();
            SoftwareComponents = new HashSet<SoftwareComponent>();
        }

        public override long Id { get; set; }

        public BuildSpecificationTypeConstant BuildSpecificationType { get; set; }

        public long? ParentId { get; set; }

        public string Name { get; set; }

        public long OwnerEmployeeId { get; set; }

        public PlatformConstant? Platform { get; set; }

        public string OperatingSystemName { get; set; }

        public string OperatingSystemVersion { get; set; }

        public string WikiLink { get; set; }

        public string Overview { get; set; }

        public bool RunningCoreOs { get; set; }

        public string EmailAddress { get; set; }

        public override byte[] TimeStamp { get; set; }

        public Employee Owner { get; set; }
        public BuildSpecification Parent { get; set; }
        public ICollection<BuildSpecification> Children { get; set; }
        public ICollection<Justification> Justifications { get; set; }
        public ICollection<Port> Ports { get; set; }
        public ICollection<SoftwareComponent> SoftwareComponents { get; set; }
        public ICollection<Node> Nodes { get; set; }
    }
}
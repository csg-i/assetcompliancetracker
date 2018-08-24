using System.Collections.Generic;

namespace act.core.data
{
    public class Employee
    {
        public Employee()
        {
            BuildSpecifications = new HashSet<BuildSpecification>();
            Nodes = new HashSet<Node>();
        }

        public long Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SamAccountName { get; set; }

        public bool IsActive { get; set; }

        public string PreferredName { get; set; }

        public long? SupervisorId { get; set; }

        public long? ReportingDirectorId { get; set; }

        public string Email { get; set; }

        public Employee Supervisor { get; set; }

        public Employee ReportingDirector { get; set; }
        
        public ICollection<Node> Nodes { get; set; }
        public ICollection<BuildSpecification> BuildSpecifications { get; set; }
    }
}
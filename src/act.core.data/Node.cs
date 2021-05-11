using System;
using System.Collections.Generic;

namespace act.core.data
{
    public class Node
    {
        public Node()
        {
            ComplianceResults = new HashSet<ComplianceResult>();
        }

        public long InventoryItemId { get; set; }

        public string Fqdn { get; set; }

        public long OwnerEmployeeId { get; set; }

        public string ProductCode { get; set; }

        public int FunctionId { get; set; }

        public PciScopeConstant PciScope { get; set; }

        public int EnvironmentId { get; set; }

        public bool IsActive { get; set; }

        public DateTime? DeactivatedDate { get; set; }

        public long? BuildSpecificationId { get; set; }
        
        public PlatformConstant Platform { get; set; }

        public ComplianceStatusConstant ComplianceStatus { get; set; }

        public DateTime? LastComplianceResultDate { get; set; }
        public DateTime? FailingSince { get; set; }
        public DateTime? LastEmailedOn { get; set; }

        public Guid? ChefNodeId { get; set; }

        public Guid? LastComplianceResultId { get; set; }

        public string RemedyGroupName { get; set; }

        public string RemedyGroupEmailList { get; set; }

        public Employee Owner { get; set; }

        public Product Product { get; set; }

        public Function Function { get; set; }

        public Environment Environment { get; set; }
        
        public BuildSpecification BuildSpecification { get; set; }

        public ICollection<ComplianceResult> ComplianceResults { get; set; }
    }
}
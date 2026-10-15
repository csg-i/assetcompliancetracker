using System;

namespace act.core.etl.csg
{
    public class NodeData
    {
        public long GamInventoryId { get; set; }
        public long? AppSpecId { get; set; }
        public string AppSpecName { get; set; }
        public string AppSpecOwnerLanId{ get; set; }
        public string AppSpecOwnerName { get; set; }
        public long? OsSpecId { get; set; }
        public string OsSpecName { get; set; }
        public string OsSpecOwnerLanId { get; set; }
        public string OsSpecOwnerName { get; set; }
        public string Os { get; set; }
        public string OsVersion{ get; set; }
        public string NodeEnvironmentName { get; set; }
        public Guid? ChefNodeId { get; set; }
        public string ComplianceStatus{ get; set; }
        public DateTime? LastComplianceResultDate{ get; set; }
        public string Fqdn{ get; set; }
        public bool IsAppliance { get; set; }
        public bool IsUnixPhysical { get; set; }
        public string PciScope { get; set; }
        public string GamTechContactLanId { get; set; }
        public string GamTechContactName { get; set; }

        public string RemedyGroupName { get; set; }

        public string RemedyGroupEmailList { get; set; }
    }
}
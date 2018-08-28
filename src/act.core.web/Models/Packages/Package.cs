using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.Packages
{
    public class Package
    {
        public Package(BuildSpecificationTypeConstant buildSpecificationType, JustificationTypeConstant packageType,
            long id, string name, string description, bool nonCore, long? justificationId, PciScopeConstant? pciScope,
            int[] environmentIds, string environmentDescription)
        {
            BuildSpecificationType = buildSpecificationType;
            PackageType = packageType;
            Id = id;
            Name = name;
            Description = description;
            NonCore = nonCore;
            JustificationId = justificationId;
            PciScope = pciScope;
            EnvironmentIds = environmentIds;
            EnvironmentDescription = environmentDescription;
        }

        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public JustificationTypeConstant PackageType { get; }
        public long Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool NonCore { get; }
        public long? JustificationId { get; }
        public PciScopeConstant? PciScope { get; }
        public int[] EnvironmentIds { get; }
        public string EnvironmentDescription { get; }

    }
}
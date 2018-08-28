using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.Packages
{
    public class EditPackage:PackageBase
    {
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public string Name { get; }
        public string Description { get; }
        public bool NonCore { get; }
        public PciScopeConstant? PciScope { get; }
        public int[] EnvironmentIds { get; }
        public IDictionary<int, (string name, string color)> AllEnvironments { get; }


        public EditPackage(BuildSpecificationTypeConstant buildSpecificationType, JustificationTypeConstant packageType, string name, string description, bool nonCore, PciScopeConstant? pciScope, int[] environmentIds, IDictionary<int, (string name, string color)> allEnvironments):base(packageType)
        {
            BuildSpecificationType = buildSpecificationType;
            Name = name;
            Description = description;
            NonCore = nonCore;
            PciScope = pciScope;
            EnvironmentIds = environmentIds;
            AllEnvironments = allEnvironments;
        }
    }
}
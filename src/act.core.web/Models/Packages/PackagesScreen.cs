using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.Packages
{
    public class PackagesScreen
    { 
        public string FriendlyName => PackageType.ToString();

        public string FriendlyNameLower => FriendlyName.ToLower();
        public string FriendlyNameLowerPlural => $"{FriendlyNameLower}s";
        public string FriendlyNamePlural => $"{FriendlyName}s";

        public JustificationTypeConstant PackageType { get; }
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public long SpecId { get; }

        public IEnumerable<Package> Packages { get; }

        public PackagesScreen(BuildSpecificationTypeConstant buildSpecificationType, JustificationTypeConstant type, long specId, IEnumerable<Package> existing)
        {
            BuildSpecificationType = buildSpecificationType;
            SpecId = specId;
            PackageType = type;
            Packages = existing;
        }
    }
}
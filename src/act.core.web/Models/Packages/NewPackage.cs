using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.Packages
{
    public class NewPackage:PackageBase
    {
        public NewPackage(JustificationTypeConstant packageType, BuildSpecificationTypeConstant buildSpecificationType, long specId, IDictionary<int, (string name, string color)> allEnvironments):base(packageType)
        {
            BuildSpecificationType = buildSpecificationType;
            SpecId = specId;
            AllEnvironments = allEnvironments;
        }

        protected NewPackage(JustificationTypeConstant packageType, BuildSpecificationTypeConstant buildSpecificationType, long specId):this(packageType, buildSpecificationType, specId, new Dictionary<int,(string name, string color)>())
        {
        }

        
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public long SpecId { get; }
        public IDictionary<int, (string name, string color)> AllEnvironments { get; }
    }
}
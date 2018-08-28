using System;
using System.Linq;
using act.core.data;

namespace act.core.web.Models.Packages
{
    public abstract class PackageBase
    {
        protected PackageBase(JustificationTypeConstant packageType)
        {
            PackageType = packageType;
            switch (packageType)
            {
                case JustificationTypeConstant.Application:
                    Example = "Chef Client 12.33.1";
                    break;
                case JustificationTypeConstant.Feature:
                    Example = "NET-Framework-45-Core";
                    break;
                case JustificationTypeConstant.Package:
                    Example = "chefclient";
                    break;
            }
            
            AllPciScopes = Enum.GetValues(typeof(PciScopeConstant)).Cast<PciScopeConstant>().ToArray();
        }
        public string FriendlyName => PackageType.ToString();

        public string Example { get; }

        public JustificationTypeConstant PackageType { get; }
        public PciScopeConstant[] AllPciScopes { get; }

    }
}
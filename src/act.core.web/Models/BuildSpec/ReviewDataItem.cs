using System;
using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class ReviewDataItem
    {
        public Guid ChefId { get; }
        public string Fqdn { get; }
        public int EnvironmentId { get; }
        public string EnvironmentName { get; }
        public PciScopeConstant PciScope { get; }
        public string Product { get; }
        public string Function { get; }
        public string Owner { get; }
        public string BuildSpec { get; }


        public ReviewDataItem(Guid chefId, string fqdn, int environmentId, string environmentName, PciScopeConstant pciScope, string product, string function,string owner, string buildSpec)
        {
            ChefId = chefId;
            Fqdn = fqdn;
            EnvironmentId = environmentId;
            EnvironmentName = environmentName;
            PciScope = pciScope;
            Product = product;
            Function = function;
            Owner = owner;
            BuildSpec = buildSpec;
        }
        
    }
}
using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class ReportingNode
    {
        
        public string Director { get; }
        public string Owner { get; }
        public string Fqdn { get; }
        public PciScopeConstant PciScope { get; }

        public ReportingNode(string director, string owner, string fqdn, PciScopeConstant pciScope)
        {
            Director = director;
            Owner = owner;
            Fqdn = fqdn;
            PciScope = pciScope;
        }
    }
}
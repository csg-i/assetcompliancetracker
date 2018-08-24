using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class Software
    {
        public string Name { get; }
        public string Description { get; }

        public JustificationTypeConstant JustificationType { get; }
        public bool NonCore { get; }
        public PciScopeConstant? PciScope { get; }
        public string Environments { get; }

        public Software(string name, string description, JustificationTypeConstant justificationType, bool nonCore, PciScopeConstant? pciScope, string environments)
        {
            Name = name;
            Description = description;
            JustificationType = justificationType;
            NonCore = nonCore;
            PciScope = pciScope;
            Environments = environments;
        }
    }
}
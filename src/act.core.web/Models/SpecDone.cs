using act.core.data;

namespace act.core.web.Models
{
    public class SpecDone
    {
        public long SpecId { get; }
        public BuildSpecificationTypeConstant Type { get; }

        public string TypeString => Type == BuildSpecificationTypeConstant.OperatingSystem ? "OS" : Type.ToString();

        public SpecDone(long specId, BuildSpecificationTypeConstant type)
        {
            SpecId = specId;
            Type = type;
        }
        
    }
}
using act.core.data;

namespace act.core.web.Models
{
    public class SpecWizard
    {
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public long? Id { get; }
        public bool FromClone { get; }

        public SpecWizard(BuildSpecificationTypeConstant buildSpecificationType,long? id, bool fromClone)
        {
            BuildSpecificationType = buildSpecificationType;
            Id = id;
            FromClone = fromClone;
        }
    }
}
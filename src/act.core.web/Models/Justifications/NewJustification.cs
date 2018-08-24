using act.core.data;

namespace act.core.web.Models.Justifications
{
    public class NewJustification
    {
       
        public NewJustification(long buildSpecid, JustificationTypeConstant justificationType)
        {
            SpecId = buildSpecid;
            JustificationType = justificationType;
        }
        public long SpecId { get;  }
        public JustificationTypeConstant JustificationType { get; }
    }
}
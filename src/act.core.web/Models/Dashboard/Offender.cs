using act.core.data;

namespace act.core.web.Models.Dashboard
{
    public class Offender
    {
        public Offender(JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name, int count)
        {
           
            Count = count;
            string type;
            switch (resultType)
            {
                case JustificationTypeConstant.Feature:
                    type = "Win-Ftr";
                    break;

                case JustificationTypeConstant.Application:
                    type = "App";
                    break;

                case JustificationTypeConstant.Package:
                    type = "Pkg";
                    break;

                default:
                    type = portType?.ToString().ToUpper() ?? "Port";
                    break;

            }

            string exists = shouldExist ? "installed" : "in spec";
            Description = $"{type} {name} not {exists}";
        }
        
        public int Count { get; }

        public string Description { get; }
    }
}
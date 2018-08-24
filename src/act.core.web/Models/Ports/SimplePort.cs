using act.core.data;

namespace act.core.web.Models.Ports
{
    public class SimplePort
    {
        public string Ports { get; }
        public PortTypeConstant Type { get; }
        public SimplePortDirectionTypeConstant Direction { get; }
        public string Justification { get; }
        public long JustificationId { get; }

        public SimplePort(string ports, PortTypeConstant type, bool external, bool outgoing, string justification, long justificationId)
        {
            Ports = ports;
            Type = type;
            if (outgoing)
            {
                Direction = SimplePortDirectionTypeConstant.SendingTrafficToOusidePort;   
            }
            else if (external)
            {
                Direction = SimplePortDirectionTypeConstant.PortListeningToOutsideTraffic;
            }
            else
            {
                Direction = SimplePortDirectionTypeConstant.PortListeningToInsideTraffic;
            }
            Justification = justification;
            JustificationId = justificationId;
        }
    }
}
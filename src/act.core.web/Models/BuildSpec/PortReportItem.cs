using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class PortReportItem
    {
        public string Port { get; }
        public bool Outgoing { get; }
        public bool External { get; }
        public PortTypeConstant PortType { get; }
        public string Justifier { get; }
        public string Justification { get; }
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }

        public string BuildSpecName { get; }

        public PortReportItem(string port, bool outgoing, bool external, PortTypeConstant portType, string justifier,
            string justification, BuildSpecificationTypeConstant buildSpecificationType, string buildSpecName)
        {
            Port = port;
            Outgoing = outgoing;
            External = external;
            PortType = portType;
            Justifier = justifier;
            Justification = justification;
            BuildSpecificationType = buildSpecificationType;
            BuildSpecName = buildSpecName;
        }
    }
}
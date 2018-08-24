using act.core.data;

namespace act.core.web.Models.Ports
{
    public class PortsScreen
    {
        public PlatformConstant Platform { get; }
        public long SpecId { get; }

        public PortsScreen(PlatformConstant platform, long specId)
        {
            Platform = platform;
            SpecId = specId;
        }
    }
}
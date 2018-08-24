using act.core.data;

namespace act.core.web.Models.Ports
{
    public class NewPort
    {
        public PlatformConstant Platform { get; }
        public long SpecId { get; }

        public NewPort(PlatformConstant platform, long specId)
        {
            Platform = platform;
            SpecId = specId;
        }
    }
}
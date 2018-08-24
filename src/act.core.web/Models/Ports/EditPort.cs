using act.core.data;

namespace act.core.web.Models.Ports
{
    public class EditPort : NewPort
    {
        public SimplePort Port { get; }

        public EditPort(PlatformConstant platform, long specId, SimplePort port) : base(platform, specId)
        {
            Port = port;
        }
    }
}
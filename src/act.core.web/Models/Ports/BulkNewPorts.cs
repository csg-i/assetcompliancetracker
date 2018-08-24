using act.core.data;

namespace act.core.web.Models.Ports
{
    public class BulkNewPorts : NewPort
    {
        public BulkNewPorts(PlatformConstant platform, long specId) : base(platform, specId)
        {
        }
    }
}
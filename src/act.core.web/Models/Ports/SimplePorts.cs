using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.Ports
{
    public class SimplePorts : List<ViewPort>
    {
        public PlatformConstant Platform { get; }

        public SimplePorts(PlatformConstant platform, IEnumerable<ViewPort> ports) : base(ports)
        {
            Platform = platform;
        }

        public bool IsEmpty => Count == 0;
    }
}
using act.core.data;
using Microsoft.AspNetCore.Html;

namespace act.core.web.Models.Ports
{
    public class ViewPort:EditPort
    {
        public ViewPort(PlatformConstant platform, long specId, SimplePort port) : base(platform, specId, port)
        {
        }

        public HtmlString PortsHtml => new HtmlString(Port.Ports.Trim().Replace("\r","").Replace("\n", ", "));
        public HtmlString JustificationHtml => new HtmlString(Port.Justification.Trim().Replace("\r", "").Replace("\n", "<br/>"));
    }
}
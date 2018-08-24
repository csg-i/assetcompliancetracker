using System.Collections.Generic;

namespace act.core.web.Models.BuildSpec
{
    public class PortReportItems:List<PortReportItem>
    {
        public PortReportItems(IEnumerable<PortReportItem> collection) : base(collection)
        {
        }
    }
}
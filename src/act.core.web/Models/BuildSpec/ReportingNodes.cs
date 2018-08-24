using System.Collections.Generic;

namespace act.core.web.Models.BuildSpec
{
    public class ReportingNodes:List<ReportingNode>
    {
        public ReportingNodes(IEnumerable<ReportingNode> collection):base(collection)
        {
            
        }
    }
}
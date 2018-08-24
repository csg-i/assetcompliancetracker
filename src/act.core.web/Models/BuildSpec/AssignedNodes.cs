using System.Collections.Generic;

namespace act.core.web.Models.BuildSpec
{
    public class AssignedNodes : List<AssignedNode>
    {
        public AssignedNodes(IEnumerable<AssignedNode> nodes):base(nodes)
        {
            
        }
    }
}
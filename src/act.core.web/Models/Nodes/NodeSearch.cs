using System.Collections.Generic;

namespace act.core.web.Models.Nodes
{
    public class NodeSearch:NodeSearchCommon
    {
        public NodeSearch(IDictionary<int, (string name, string color)> environments) : base(true, environments)
        {
        }
    }
}
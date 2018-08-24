using System.Collections.Generic;
using System.Linq;
using act.core.web.Models.AppSpecs;

namespace act.core.web.Models.Nodes
{
    public class NodesForApp : NodeSearchCommon
    {
        public AppSpecInformation Information { get; }
        public IEnumerable<NodeSearchResult> Nodes { get; }
        public long[] NodeIds { get; }

        public NodesForApp(AppSpecInformation information, NodeSearchResult[] nodes, IDictionary<int, (string name, string color)> environments):base(false, environments)
        {
            Information = information;
            Nodes = nodes;
            NodeIds = nodes.Select(p => p.Id).ToArray();
        }
    }
}
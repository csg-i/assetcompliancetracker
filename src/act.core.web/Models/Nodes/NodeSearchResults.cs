using System.Collections.Generic;

namespace act.core.web.Models.Nodes
{
    public class NodeSearchResults : List<NodeSearchResult>
    {
        public int MatchCount { get; }

        public NodeSearchResults(IEnumerable<NodeSearchResult> collection, int matchCount, int displayCount) : base(collection)
        {
            MatchCount = matchCount;
            DisplayCount = displayCount;
        }

        public int DisplayCount { get; }
        public bool Empty => Count == 0;
    }
}
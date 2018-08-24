using System.Collections.Generic;

namespace act.core.web.Models.ScoreCard
{
    public class PlatformScoreCard : List<PlatformScoreCardRow>
    {
        public PlatformScoreCard(IEnumerable<PlatformScoreCardRow> collection) : base(collection)
        {

        }
        public bool Empty => Count == 0;
    }
}
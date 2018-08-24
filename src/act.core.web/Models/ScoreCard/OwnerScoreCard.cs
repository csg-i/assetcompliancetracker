using System.Collections.Generic;

namespace act.core.web.Models.ScoreCard
{
    public class OwnerScoreCard:List<OwnerScoreCardRow>
    {
        public long EmployeeId { get; }
        public string Owner { get; }

        public OwnerScoreCard(long employeeId, string owner, IEnumerable<OwnerScoreCardRow> collection) : base(collection)
        {
            EmployeeId = employeeId;
            Owner = owner;
        }

        public bool Empty => Count == 0;
    }
}
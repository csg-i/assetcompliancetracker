using System.Collections.Generic;

namespace act.core.web.Models.ScoreCard
{
    public class ExecutiveScoreCard : List<ExecutiveScoreCardRow>
    {
        public string Supervisor { get; }
        public long? SupervisorId { get; }
        public long EmployeeId { get; }
        public string Owner { get; }

        public ExecutiveScoreCard(long employeeId, string owner, long? supervisorId, string supervisor, IEnumerable<ExecutiveScoreCardRow> collection) : base(collection)
        {
            EmployeeId = employeeId;
            Owner = owner;
            Supervisor = supervisor;
            SupervisorId = supervisorId;
        }

        public bool Empty => Count == 0;
    }
}
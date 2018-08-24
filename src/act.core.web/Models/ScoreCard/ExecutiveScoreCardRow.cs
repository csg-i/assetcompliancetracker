namespace act.core.web.Models.ScoreCard
{
    public class ExecutiveScoreCardRow
    {
        public long EmployeeId { get; }
        public string Name { get; }
        public int SpecCount { get; }
        public ScoreCardPciCount AssignedNodes { get; }
        public ScoreCardPciCount TotalNodesInInventory { get; }
        public ScoreCardPciCount PassingNodes { get; }
        public ScoreCardPciCount FailingNodes { get; }
        public ScoreCardPciCount NotReportingNodes { get; }
        public ScoreCardPciCount OutOfScopeNodes { get; }

        public int DirectReportCount { get; }
        
        public ExecutiveScoreCardRow(long employeeId, string name, int specCount, ScoreCardPciCount assignedNodes, ScoreCardPciCount totalNodesInInventory, ScoreCardPciCount passingNodes, ScoreCardPciCount failingNodes, ScoreCardPciCount notReportingNodes, ScoreCardPciCount outOfScopeNodes, int directReportCount)
        {
            EmployeeId = employeeId;
            Name = name;
            SpecCount = specCount;
            AssignedNodes = assignedNodes;
            TotalNodesInInventory = totalNodesInInventory;
            PassingNodes = passingNodes;
            FailingNodes = failingNodes;
            NotReportingNodes = notReportingNodes;
            OutOfScopeNodes = outOfScopeNodes;
            DirectReportCount = directReportCount;
        }

    }
}
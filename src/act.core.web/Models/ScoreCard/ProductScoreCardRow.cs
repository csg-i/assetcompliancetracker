namespace act.core.web.Models.ScoreCard
{
    public class ProductScoreCardRow:IProductScoreCard
    {
        public ProductScoreCardRow(int functionId, string function, long ownerId, string owner, long? directorId, string director, int specCount, ScoreCardPciCount allNodes, ScoreCardPciCount assignedNodes, ScoreCardPciCount passingNodes, ScoreCardPciCount failingNodes, ScoreCardPciCount notReportingNodes, ScoreCardPciCount outOfChefScopeNodes)
        {
            FunctionId = functionId;
            Function = function;
            OwnerId = ownerId;
            Owner = owner;
            DirectorId = directorId;
            Director = director;
            SpecCount = specCount;
            AllNodes = allNodes;
            AssignedNodes = assignedNodes;
            PassingNodes = passingNodes;
            FailingNodes = failingNodes;
            NotReportingNodes = notReportingNodes;
            OutOfChefScopeNodes = outOfChefScopeNodes;
            UnassignedNodes = allNodes - assignedNodes;
        }

        internal void SetCode(ProductScoreCard sc)
        {
            Code = sc?.Code;
            Description = sc?.Description;
        }

        public string Code { get; private set; }
        public string Description { get; private set; }
        public int? FunctionId { get; }
        public string Function { get; }
        public long? OwnerId { get; }
        public string Owner { get; }
        public long? DirectorId { get; }
        public string Director { get; }
        public int SpecCount { get; }
        public ScoreCardPciCount AllNodes { get; }
        public ScoreCardPciCount AssignedNodes { get; }
        public ScoreCardPciCount PassingNodes { get; }
        public ScoreCardPciCount FailingNodes { get; }
        public ScoreCardPciCount NotReportingNodes { get; }
        public ScoreCardPciCount OutOfChefScopeNodes { get; }
        public ScoreCardPciCount UnassignedNodes { get; }
    }
}
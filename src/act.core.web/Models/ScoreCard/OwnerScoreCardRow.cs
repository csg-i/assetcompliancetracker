using act.core.data;

namespace act.core.web.Models.ScoreCard
{
    public class OwnerScoreCardRow
    {
        public long Id { get; }
        public string SpecName { get; }
        public long? ParentId { get; }
        public string ParentName { get; }
        public BuildSpecificationTypeConstant BuildSpecificationType { get; }
        public PlatformConstant Platform { get; }
        public int NodeCount { get; }
        public int PassingNodeCount { get; }
        public int FailingNodeCount { get; }
        public ScoreCardCount TotalSoftwareCount { get; }
        public ScoreCardCount UnjustifiedSoftwareCount { get; }
        public ScoreCardCount PortCount { get; }

        public OwnerScoreCardRow(long id, string specName, long? parentId, string parentName, BuildSpecificationTypeConstant buildSpecificationType, PlatformConstant platform, int nodeCount, int passingNodeCount, int failingNodeCount,
            ScoreCardCount totalSoftwareCount, ScoreCardCount unjustifiedSoftwareCount, ScoreCardCount portCount)
        {
            Id = id;
            SpecName = specName;
            ParentId = parentId;
            ParentName = parentName;
            BuildSpecificationType = buildSpecificationType;
            Platform = platform;
            NodeCount = nodeCount;
            PassingNodeCount = passingNodeCount;
            FailingNodeCount = failingNodeCount;
            TotalSoftwareCount = totalSoftwareCount;
            UnjustifiedSoftwareCount = unjustifiedSoftwareCount;
            PortCount = portCount;
        }
    }
}
namespace act.core.web.Models.ScoreCard
{
    public class PlatformScoreCardRow
    {
        public long Id { get; }
        public string SpecName { get; }
        public long OwnerId { get; }
        public string Owner { get; }
        public string OsName { get; }
        public string OsVersion { get; }
        public int NodeCount { get; }
        public int AppSpecCount { get; }

        public PlatformScoreCardRow(long id, string specName, long ownerId, string owner, string osName, string osVersion,
            int nodeCount, int appSpecCount)
        {
            Id = id;
            SpecName = specName;
            OwnerId = ownerId;
            Owner = owner;
            OsName = osName;
            OsVersion = osVersion;
            NodeCount = nodeCount;
            AppSpecCount = appSpecCount;
        }
    }
}
namespace act.core.web.Models.ScoreCard
{
    public struct ScoreCardCount
    {
        public int OsCount { get; }
        public int AppCount { get; }

        public ScoreCardCount(int osCount, int appCount)
        {
            OsCount = osCount;
            AppCount = appCount;
        }        

        public static  ScoreCardCount operator +(ScoreCardCount n1, ScoreCardCount n2)
        {
            return new ScoreCardCount(n1.OsCount + n2.OsCount, n1.AppCount +n2.AppCount);
        }
    }
}
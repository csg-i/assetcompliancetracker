using System;

namespace act.core.web.Models.ScoreCard
{
    public struct ScoreCardPciCount
    {
        public int PciCount { get; }
        public int TotalCount { get; }

        public ScoreCardPciCount(int pciCount, int totalCount)
        {
            PciCount = pciCount;
            TotalCount = totalCount;
        }

        public static ScoreCardPciCount operator +(ScoreCardPciCount p1, ScoreCardPciCount p2)
        {
            return new ScoreCardPciCount(p1.PciCount + p2.PciCount, p1.TotalCount + p2.TotalCount);
        }
        public static ScoreCardPciCount operator -(ScoreCardPciCount p1, ScoreCardPciCount p2)
        {
            return new ScoreCardPciCount(p1.PciCount - p2.PciCount, p1.TotalCount - p2.TotalCount);
        }
        public static ScoreCardPciCount operator /(ScoreCardPciCount p1, ScoreCardPciCount p2)
        {
            if (p2.TotalCount == 0)
                return new ScoreCardPciCount(0, 0);
            if(p2.PciCount == 0)
                return new ScoreCardPciCount(0, Convert.ToInt32((Convert.ToDecimal(p1.TotalCount) / p2.TotalCount) * 100m));

            return new ScoreCardPciCount(Convert.ToInt32((Convert.ToDecimal(p1.PciCount) / p2.PciCount)*100m), Convert.ToInt32((Convert.ToDecimal(p1.TotalCount) / p2.TotalCount)*100m));
        }
    }
}
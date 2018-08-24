using act.core.web.Models.ScoreCard;

namespace act.core.web.Models.Dashboard
{
    public class Status
    {
        public Status(ScoreCardPciCount passing, ScoreCardPciCount failingCompliance, ScoreCardPciCount unassigned, ScoreCardPciCount notReporting, ScoreCardPciCount unix, ScoreCardPciCount osOther, ScoreCardPciCount appliance, ScoreCardPciCount productExcluded)
        {
            Passing = passing;
            FailingCompliance = failingCompliance;
            Unassigned = unassigned;
            NotReporting = notReporting;
            OutOfScope = unix + osOther + appliance + productExcluded;
            Unix = unix;
            OsOther = osOther;
            Applicance = appliance;
            ProductExcluded = productExcluded;
            TotalFailing = FailingCompliance + NotReporting + Unassigned;
            InScope = Passing + TotalFailing;
            PercentPassingInScope = Passing / InScope;
        }

        public ScoreCardPciCount Passing { get; }
        public ScoreCardPciCount FailingCompliance { get; }
        public ScoreCardPciCount Unassigned { get; }
        public ScoreCardPciCount NotReporting { get; }
        public ScoreCardPciCount OutOfScope { get; }
        public ScoreCardPciCount Unix { get; }
        public ScoreCardPciCount OsOther { get; }
        public ScoreCardPciCount Applicance { get; }
        public ScoreCardPciCount ProductExcluded { get; }
        public ScoreCardPciCount InScope { get; }
        public ScoreCardPciCount TotalFailing { get; }
        public ScoreCardPciCount PercentPassingInScope { get; }
    }
}
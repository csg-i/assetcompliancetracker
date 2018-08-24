namespace act.core.data
{
    public class ComplianceResultError
    {
        public long Id { get; set; }

        public long ComplianceResultId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string LongMessage { get; set; }

        public ComplianceStatusConstant Status { get; set; }

        public ComplianceResult ComplianceResult { get; set; }
    }
}
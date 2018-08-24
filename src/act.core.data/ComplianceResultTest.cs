namespace act.core.data
{
    public class ComplianceResultTest
    {
        public long Id { get; set; }

        public long ComplianceResultId { get; set; }

        public JustificationTypeConstant ResultType { get; set; }

        public bool ShouldExist { get; set; }

        public string Name { get; set; }

        public ComplianceStatusConstant Status { get; set; }

        public PortTypeConstant? PortType { get; set; }

        public ComplianceResult ComplianceResult { get; set; }
    }
}
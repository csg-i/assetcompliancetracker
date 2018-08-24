namespace act.core.data
{
    public class Port : BuildSpecReference
    {
        public override long Id { get; set; }

        public override long BuildSpecificationId { get; set; }

        public PortTypeConstant PortType { get; set; }

        public int From { get; set; }

        public int? To { get; set; }

        public override byte[] TimeStamp { get; set; }

        public bool IsExternal { get; set; }

        public bool IsOutgoing { get; set; }

        public long? JustificationId { get; set; }

        public BuildSpecification BuildSpecification { get; set; }

        public Justification Justification { get; set; }
    }
}
namespace act.core.web.Models
{
    public class Counts
    {
        public int Ports { get; }
        public int JustifiedSoftware { get; }
        public int TotalSoftware { get; }
        public decimal PercentComplete => (Ports > 0 ? .15m : 0m) + .85m * (TotalSoftware > 0 ? new decimal(JustifiedSoftware) / new decimal(TotalSoftware) : 0m);

        public Counts(int ports, int justifiedSoftware, int totalSoftware)
        {
            Ports = ports;
            JustifiedSoftware = justifiedSoftware;
            TotalSoftware = totalSoftware;
        }
    }
}
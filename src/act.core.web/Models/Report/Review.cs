using System.Collections.Generic;

namespace act.core.web.Models.Report
{
    public class Review
    {
        public long SpecId { get; }
        public int? EnvironmentId { get; }
        public  IDictionary<int,(string name, string color)> Environments { get; }

        public Review(long specId, int? environmentId, IDictionary<int,(string name, string color)> environments)
        {
            SpecId = specId;
            EnvironmentId = environmentId;
            Environments = environments;
        }
    }
}
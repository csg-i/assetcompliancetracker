using System.Collections.Generic;
using System.Linq;

namespace act.core.web.Models.Dashboard
{
    public class Spread
    {
        public Spread(IEnumerable<Stat> osSpecs, IEnumerable<Stat> productSpecs)
        {
            OsSpecs = osSpecs.OrderByDescending(p=>p.SpecCount).ToArray();
            ProductSpecs = productSpecs.Where(p=>p.SpecCount > 1).OrderByDescending(p=>p.SpecCount).ToArray();
        }

        public Stat[] OsSpecs { get; }
        public Stat[] ProductSpecs { get; }
    }
}
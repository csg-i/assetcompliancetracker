using System.Collections.Generic;

namespace act.core.web.Models.Dashboard
{
    public class TopOffenders:List<Offender>
    {
        public int OsFailures { get; }

        public TopOffenders(int osFailures, IEnumerable<Offender> collection) : base(collection)
        {
            OsFailures = osFailures;
        }

        public bool Empty=> Count ==0;
    }
}
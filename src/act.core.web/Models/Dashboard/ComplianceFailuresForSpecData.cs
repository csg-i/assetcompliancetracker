using System;
using System.Collections.Generic;
using System.Linq;
using act.core.data;

namespace act.core.web.Models.Dashboard
{
    public class ComplianceFailuresForSpec : List<ComplianceFailuresForSpecData>
    {
        public string[] Categories { get; }
        public ComplianceFailuresForSpec(IDictionary<DateTime, ComplianceFailuresForSpecData> data) : base(data.Values.OrderBy(p => p.Number))
        {
            Categories = data.Keys.OrderBy(p => p.Date).Select(p => p.ToShortDateString()).ToArray();
        }

        public IEnumerable<int[]> GetByType(JustificationTypeConstant status)
        {
            foreach (var data in this)
            {
                yield return new[] { data.Number, data.Counts[status] };
            }
        }
    }

    public class ComplianceFailuresForSpecData
    {
        public ComplianceFailuresForSpecData(int number, DateTime date)
        {
            Number = number;
            Date = date;
            Counts = new Dictionary<JustificationTypeConstant, int>();
        }

        public int Number { get; }
        public DateTime Date { get; }
        public IDictionary<JustificationTypeConstant, int> Counts { get; }
    }
}
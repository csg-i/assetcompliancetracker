using System;
using System.Collections.Generic;
using System.Linq;
using act.core.data;

namespace act.core.web.Models.Dashboard
{
    public class ComplianceOverTime:List<ComplianceOverTimeData>
    {
        public enum EmployeeFilterConstant
        {
            Director,
            Owner
        }
        public string[] Categories { get; }
        public int[][] Weekends { get; }
        public ComplianceOverTime(IDictionary<DateTime,ComplianceOverTimeData> data) : base(data.Values.OrderBy(p=>p.Number))
        {
            Categories = data.Keys.OrderBy(p=>p.Date).Select(p => p.ToShortDateString()).ToArray();
        }

        public IEnumerable<int[]> GetByStatus(ComplianceStatusConstant status)
        {
            foreach (var data in this)
            {
                yield return new []{data.Number, data.Counts[status]};
            }
        }

        public IEnumerable<int[]> FindWeekendGroups()
        {
           var list = new List<int>();
            ComplianceOverTimeData last = null;
            foreach (var value in this)
            {
                if (value.Date.DayOfWeek == DayOfWeek.Saturday)
                {
                    list = new List<int> {value.Number};
                } 
                else if (value.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (last == null || last.Number != (value.Number - 1))
                    {
                        list = new List<int> {value.Number -1, value.Number};
                    }
                    else
                    {
                        list.Add(value.Number);
                    }

                    yield return list.ToArray();
                }
                last = value;
            }

            if (list.Any())
            {
                if (list.Count == 1)
                    list.Add(list[0] + 1);
                yield return list.ToArray();
            }
        }
    }
    

    public class ComplianceOverTimeData
    {
        public ComplianceOverTimeData(int number, DateTime date)
        {
            Number = number;
            Date = date;
            Counts = new Dictionary<ComplianceStatusConstant, int>();
        }

        public int Number { get; }
        public DateTime Date { get; }
        public IDictionary<ComplianceStatusConstant, int> Counts { get; }
    }
}
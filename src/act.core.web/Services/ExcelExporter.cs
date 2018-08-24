using System.Collections.Generic;
using System.Reflection;
using System.Text;
using act.core.web.Models.ScoreCard;

namespace act.core.web.Services
{
    internal class ExcelExporter : IExcelExporter
    {
        public byte[] Export<T>(IEnumerable<T> rows) where T : class
        {
            var propertyInfos = typeof(T).GetProperties
                (BindingFlags.Public | BindingFlags.Instance);

            var data = Combine(rows, propertyInfos);
            return Encoding.ASCII.GetBytes(data);
        }

        private static string QuoteString(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return value;
            if (!value.Contains(","))
                return value;
            
            return $"\"{value.Replace("\"","\\\"")}\"";
        }

        private static string Combine<T>(IEnumerable<T> rows, PropertyInfo[] propertyInfos) where T : class
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(", ", GetHeaders(propertyInfos)));
            foreach (var row in rows) 
                sb.AppendLine(string.Join(", ", GetColumns(row, propertyInfos)));
            return sb.ToString();
        }

        private static IEnumerable<string> GetHeaders(IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (var pi in propertyInfos)
                if (!pi.Name.ToLower().Equals("id"))
                {
                    if (pi.PropertyType == typeof(ScoreCardCount))
                    {
                        yield return $"{pi.Name} OS";
                        yield return $"{pi.Name} App";
                    }
                    else if (pi.PropertyType == typeof(ScoreCardPciCount))
                    {
                        yield return $"{pi.Name} PCI";
                        yield return $"{pi.Name} Total";
                    }
                    else
                    {
                        yield return pi.Name;
                    }
                }
        }

        private static IEnumerable<string> GetColumns<T>(T row, IEnumerable<PropertyInfo> propertyInfos) where T : class
        {
            foreach (var pi in propertyInfos)
                if (!pi.Name.ToLower().Equals("id"))
                {
                    var value = pi.GetValue(row);
                    if (pi.PropertyType == typeof(ScoreCardCount))
                    {
                        var scc = (ScoreCardCount) value;
                        yield return scc.OsCount.ToString();
                        yield return scc.AppCount.ToString();
                    }
                    else if (pi.PropertyType == typeof(ScoreCardPciCount))
                    {
                        var scc = (ScoreCardPciCount) value;
                        yield return scc.PciCount.ToString();
                        yield return scc.TotalCount.ToString();
                    }
                    else
                    {
                        yield return QuoteString((value ?? string.Empty).ToString());
                    }
                }
        }
    }
}
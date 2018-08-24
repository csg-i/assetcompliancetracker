using System.Collections.Generic;

namespace act.core.web.Services
{
    public interface IExcelExporter
    {
        byte[] Export<T>(IEnumerable<T> rows) where T : class;
    }
}
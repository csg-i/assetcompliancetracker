using System.Collections.Generic;

namespace act.core.web.Models.OsSpecs
{
    public class OsSpecSearchResults:List<OsSpecSearchResult>
    {
        public bool Empty => Count == 0;
    }
}
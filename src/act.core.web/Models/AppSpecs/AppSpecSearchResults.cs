using System.Collections.Generic;

namespace act.core.web.Models.AppSpecs
{
    public class AppSpecSearchResults:List<AppSpecSearchResult>
    {
        public bool Empty => Count == 0;
    }
}
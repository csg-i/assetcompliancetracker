using System.Collections.Generic;

namespace act.core.web.Models.Home
{
    public class Help:List<HelpLink>
    {
        public bool IsEmpty => Count == 0;
    }
}
namespace act.core.web.Models
{
    public class JsonSpecSearchResult
    {
        // ReSharper disable InconsistentNaming
        public long i;

        public string t;

        public string p;
        // ReSharper restore InconsistentNaming

        // ReSharper disable UnusedMember.Global
        public JsonSpecSearchResult()           
        {
            
        }
        // ReSharper restore UnusedMember.Global
        public JsonSpecSearchResult(long id, string name, string platform)
        {
            i = id;
            t = name;
            p = platform;
        }
    }
}
namespace act.core.web.Models.Employees
{
    public class JsonEmployeeSearchResult
    {
        // ReSharper disable InconsistentNaming
        public long i;

        public string t;
        // ReSharper restore InconsistentNaming

        // ReSharper disable UnusedMember.Global
        public JsonEmployeeSearchResult()           
        {
            
        }
        // ReSharper restore UnusedMember.Global
        public JsonEmployeeSearchResult(long id, string text)
        {
            i = id;
            t = text;
        }
    }
}
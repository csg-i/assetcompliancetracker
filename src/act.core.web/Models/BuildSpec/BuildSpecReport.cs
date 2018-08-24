namespace act.core.web.Models.BuildSpec
{
    public class BuildSpecReport
    {
        public long Id { get; }
        public string RootUri { get; }
        
        public BuildSpecReport(long id, string rootUri)
        {
            Id = id;
            RootUri = rootUri;
        }
    }
}
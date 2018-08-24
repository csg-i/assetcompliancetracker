namespace act.core.web.Models.BuildSpec
{
    public class AssignedNode
    {
        public string BuildSpec { get; }
        public string Fqdn { get; }

        public AssignedNode(string buildSpec, string fqdn)
        {
            BuildSpec = buildSpec;
            Fqdn = fqdn;
        }
    }
}
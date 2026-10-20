using Newtonsoft.Json;

namespace act.core.etl.ComplianceModel
{
    public class CfgmgmtNode
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fqdn")]
        public string Fqdn { get; set; }

        [JsonProperty("chef_version")]
        public string ChefVersion { get; set; }

        [JsonProperty("policy_name")]
        public string PolicyName { get; set; }

        [JsonProperty("policy_group")]
        public string PolicyGroup { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public string NodeName => !string.IsNullOrWhiteSpace(Fqdn) ? Fqdn : Name;
    }

    public class CfgmgmtNodes
    {
        [JsonProperty("nodes")]
        public CfgmgmtNode[] Nodes { get; set; }
    }
}

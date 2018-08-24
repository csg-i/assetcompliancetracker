using System.Linq;
using act.core.data;
using Microsoft.AspNetCore.Html;

namespace act.core.web.Models.BuildSpec
{
    public class BuildSpec
    {
        public long Id { get; }
        public BuildSpecificationTypeConstant Type { get; }
        public string Name { get; }
        public string Owner { get; }
        public string OsName { get; }
        public string OsVersion { get; }
        public string WikiLink { get; }
        public PortReportItems Ports { get; }
        public HtmlString Overview { get; }
        public IOrderedEnumerable<Justification> Justifications { get; }
        public IOrderedEnumerable<Software> UnjustifiedSoftware { get; }
        public IOrderedEnumerable<GamNode> Nodes { get; }

        public BuildSpec(long id, BuildSpecificationTypeConstant type, string name, string owner,string osName, string osVersion, string wikiLink, string overview, Justification[] justifications, PortReportItems ports, Software[] unJustifiedThings, GamNode[] nodes)
        {
            Id = id;
            Type = type;
            Name = name;
            Owner = owner;
            OsName = osName;
            OsVersion = osVersion;
            WikiLink = wikiLink;
            Ports = ports;
            Overview = new HtmlString((overview??string.Empty).Replace("\n","<br/>"));
            Justifications = (justifications ?? Enumerable.Empty<Justification>()).OrderBy(p => (int)p.JustificationType);
            UnjustifiedSoftware = (unJustifiedThings ?? Enumerable.Empty<Software>()).OrderBy(p => p.JustificationType).ThenBy(p => p.Name);
            Nodes = (nodes ?? Enumerable.Empty<GamNode>()).OrderBy(p => p.Fqdn);
        }
    }
}
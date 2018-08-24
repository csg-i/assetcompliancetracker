using System;
using System.Collections.Generic;
using System.Linq;
using act.core.data;

namespace act.core.web.Models.Nodes
{
    public abstract class NodeSearchCommon
    {
        public bool NodesScreen { get; }
        public IDictionary<int, (string name, string color)> Environments { get; }
        public PlatformConstant[] AllPlatforms { get; }
        public NodeComplianceSearchTypeConstant[] AllStates { get; }
        public PciScopeConstant[] AllSecurityClasses { get; }

        protected NodeSearchCommon(bool nodesScreen, IDictionary<int, (string name, string color)> environments)
        {
            NodesScreen = nodesScreen;
            Environments = environments;
            AllPlatforms = Enum.GetValues(typeof(PlatformConstant)).Cast<PlatformConstant>().ToArray();
            AllStates = Enum.GetValues(typeof(NodeComplianceSearchTypeConstant)).Cast<NodeComplianceSearchTypeConstant>().ToArray();
            AllSecurityClasses = Enum.GetValues(typeof(PciScopeConstant)).Cast<PciScopeConstant>().ToArray();
        }
    }
}
using System;

namespace act.core.web.Models.Nodes
{
    [Flags]
    public enum NodeComplianceSearchTypeConstant
    {
        Unassigned = 1,
        Assigned = 2,
        Passing = 4,
        Failing = 8,
        NotReporting = 16 
    }
}
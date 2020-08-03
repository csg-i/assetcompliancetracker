using System;
using act.core.data;

namespace act.core.web.Models.Nodes
{
    public class NodeSearchResult
    {
        private int? _firstDot;
        public long Id { get; }
        public string Fqdn { get; }
        public string Owner { get; }
        public string Product { get; }
        public string Function { get; }
        public string SecurityClass { get; }
        public int EnvironmentId { get; }
        public string EnvironmentName { get; }
        public string EnvironmentDescription { get; }
        public string EnvironmentColor { get; }
        public PlatformConstant Platform { get; }
        public long? BuildSpecId { get; }
        public string BuildSpecName { get; }
        public ComplianceStatusConstant ComplianceStatus { get; }

        public string ComplianceStatusColor
        {
            get
            {
                switch (ComplianceStatus)
                {
                    case ComplianceStatusConstant.Failed:
                        return "danger";
                    case ComplianceStatusConstant.NotFound:
                        return "secondary";
                    case ComplianceStatusConstant.Succeeded:
                        return "success";
                }

                return "secondary";
            }
        }

        public DateTime? LastComplianceDate { get; }

        public DateTime? LastComplianceDateLocal
        {
            get
            {
                return LastComplianceDate == null? LastComplianceDate : Convert.ToDateTime(LastComplianceDate).AddMinutes(-1 * LocalTimeOffset);
            }
        }
        public int LocalTimeOffset { get; }
        public Guid? ChefId { get; }
        public bool ShowButtons { get; }

        public int FirstDot
        {
            get
            {
                if (_firstDot == null)
                    _firstDot = Fqdn.IndexOf(".", StringComparison.Ordinal);

                return _firstDot.Value;
            }
        }

        public string HostName => FirstDot > 0 ? Fqdn.Substring(0, FirstDot) : Fqdn;

        public string Domain => FirstDot > 0 ? Fqdn.Substring(FirstDot, Fqdn.Length - FirstDot) : string.Empty;

        public NodeSearchResult(long id, string fqdn, string owner, string product, string function, PciScopeConstant pciScope, int environmentId, string environmentName,  string environmentDescription, string environmentColor, PlatformConstant platform, long? buildSpecId, string buildSpecName, ComplianceStatusConstant complianceStatus, DateTime? lastComplianceDate, Guid? chefId, bool showButtons,int localTimeOffset)
        {
            Id = id;
            Fqdn = fqdn;
            Owner = owner;
            Product = product;
            Function = function;
            SecurityClass = pciScope.ToString();
            EnvironmentDescription = environmentDescription;
            EnvironmentColor = environmentColor;
            EnvironmentName = environmentName;
            EnvironmentId = environmentId;
            Platform = platform;
            BuildSpecId = buildSpecId;
            BuildSpecName = buildSpecName;
            ComplianceStatus = complianceStatus;
            LastComplianceDate = lastComplianceDate;
            ChefId = chefId;
            ShowButtons = showButtons;
            LocalTimeOffset = localTimeOffset;
        }
    }
}
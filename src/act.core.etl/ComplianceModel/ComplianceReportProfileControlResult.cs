using System.Linq;
using System.Text.RegularExpressions;
using act.core.data;

namespace act.core.etl.ComplianceModel
{ 
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global        
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class ComplianceReportProfileControlResult
    {
        // ReSharper disable MemberCanBePrivate.Global
        public string status { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public string code_desc { get; set; }
        public string message { get; set; }

        public ComplianceStatusConstant GetComplainceStatus()
        {
            if (status == "failed")
                return ComplianceStatusConstant.Failed;

            return ComplianceStatusConstant.Succeeded;
        }

        public string GetSoftwareName()
        {
            var firstQuote = code_desc.IndexOf('"') + 1;
            var @try = code_desc.Substring(firstQuote, code_desc.Length - firstQuote - 1);
            if (string.IsNullOrWhiteSpace(@try))
                return "(empty string)";
            return @try;
        }

        public JustificationTypeConstant GetSoftwareJustificationType()
        {
            if (code_desc.Contains("installed_products"))
                return JustificationTypeConstant.Application;

            if (code_desc.Contains("installed_features"))
                return JustificationTypeConstant.Feature;


            return JustificationTypeConstant.Package;
        }

        public string GetPortNumber()
        {
            var firstSquare = code_desc.IndexOf('[');
            var lastSquare = code_desc.IndexOf(']');
            var onlyPort = code_desc.Substring(firstSquare, lastSquare - firstSquare);

            return string.Join(string.Empty, Regex.Matches(onlyPort, @"\d+").Select(m => m.Value));
        }

        public PortTypeConstant? GetPortType()
        {
            if (code_desc.Contains("tcp6_ports"))
                return PortTypeConstant.Tcp6;

            if (code_desc.Contains("tcp_ports"))
                return PortTypeConstant.Tcp;

            if (code_desc.Contains("udp6_ports"))
                return PortTypeConstant.Udp6;

            if (code_desc.Contains("udp_ports"))
                return PortTypeConstant.Udp;

            return null;
        }
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
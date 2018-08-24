using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using act.core.data;
using act.core.web.Framework;
using act.core.web.Services;
using Microsoft.AspNetCore.Html;
using Port = act.core.data.Port;

namespace act.core.web.Extensions
{

    public static class ModelExtensions
    {
        internal static bool IsValidEmail(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            try
            {
                var m = new MailAddress(s);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }

        }
        internal static string OwnerText(this IUserSecurity userSecurity)
        {
            if (userSecurity == null)
                return string.Empty;

            return $"{userSecurity.FirstName} {userSecurity.LastName} ({userSecurity.SamAccountName.Trim()})";
        }
        
        internal static bool IsValidPortEntry(this string portString, out MatchCollection matches)
        {
            matches = portString.GetValidPortEntryMatches();
            if (matches == null)
                return false;

            return portString.NormalizeReturns() == matches.RebuildStringFromMatches();
        }

        private static string RebuildStringFromMatches(this MatchCollection collection)
        {
            if (collection == null)
                return null;

            var sb = new StringBuilder();
            foreach (Match m in collection)
            {
                sb.AppendLine(m.Value);
            }
            return sb.ToString();
        }

        private static string NormalizeReturns(this string portString)
        {
            if (portString == null)
                return null;

            var sb = new StringBuilder();
            var split = portString.Trim().Replace("\r", "").Split('\n');
            foreach (var m in split)
            {
                sb.AppendLine(m);
            }
            return sb.ToString();
        }

        private static MatchCollection GetValidPortEntryMatches(this string portString)
        {
            if (portString == null)
                return null;

            return Regex.Matches(portString.Trim().Replace("\r", ""), "^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5‌​])(?::([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5]))?$", RegexOptions.Multiline);
        }

        internal static DataPortValidation ValidateDataPorts(this string portString)
        {
            if (string.IsNullOrWhiteSpace(portString))
                return new DataPortValidation(false);

            var list = new List<Port>();
            MatchCollection matches;
            var valid = IsValidPortEntry(portString, out matches);
            if (matches == null)
            {
                return new DataPortValidation(valid);
            }

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    list.Add(new Port
                    {
                        From = Convert.ToInt32(match.Groups[1].Value),
                        To = match.Groups[2].Success ? Convert.ToInt32(match.Groups[2].Value) : (int?)null
                    });
                }
            }


            return new DataPortValidation(valid, list.ToArray());
        }

        public static HtmlString ToYesString(this bool b)
        {
            if(b)
                return new HtmlString(@"<span class=""sr-only"">Yes</span><i class=""fa fa-check"" title=""Yes""></i>");

            return new HtmlString(string.Empty);
        }
        public static HtmlString ToDirectionString(this bool outgoing)
        {
            if (outgoing)
                return new HtmlString(@"<span class=""sr-only"">Outgoing</span><i class=""fa fa-sign-out"" title=""Outgoing""></i>");

            return new HtmlString(@"<span class=""sr-only"">Incoming</span><i class=""fa fa-sign-in"" title=""Incoming""></i>");
        }
        public static string CardColorClass(this JustificationTypeConstant? jtc)
        {
            if (!jtc.HasValue)
                return null;

            switch (jtc.Value)
            {
                case JustificationTypeConstant.Package:
                case JustificationTypeConstant.Feature:
                    return "primary";
                case JustificationTypeConstant.Application:
                    return "warning";
                case JustificationTypeConstant.Port:
                    return "info";
                default:
                    return null;
            }
        }
        internal static string ToPortString(this IEnumerable<Port> ports)
        {
            if (ports == null)
                return null;


            var sb = new StringBuilder();
            foreach (var port in ports)
            {
                sb.AppendLine(port.ToPortString());
            }
            return sb.ToString();

        }

        internal static string ToPortString(this Port port)
        {
            if (port == null)
                return string.Empty;

            return port.To.HasValue ? $"{port.From}:{port.To}" : port.From.ToString();
        }

        public static string ToPercent(this decimal d)
        {
            return d.ToString("p0").Replace(" ", string.Empty);
        }
    }
}
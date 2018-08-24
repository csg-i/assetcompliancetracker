using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace act.core.web.Framework
{
    public static class ClaimsExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (!string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal))
                return string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
            return true;
        }
        public static bool IsAjaxHtml(this HttpRequest request)
        {
            if (!string.Equals(request.Query["Accept"], "text/html, */*; q=0.01", StringComparison.Ordinal))
                return string.Equals(request.Headers["Accept"], "text/html, */*; q=0.01", StringComparison.Ordinal);
            return true;
        }

        public static string GetClaim(this IEnumerable<Claim> claims, string type)
        {
            return claims?
                .Where(p => p.Type == type)
                .Select(p => p.Value).FirstOrDefault();
        }
        public static long GetClaimAsInt64(this IEnumerable<Claim> claims, string type)
        {
            var p = claims.GetClaim(type);
            if (p == null)
                return 0;

            long l;
            long.TryParse(p, out l);
            return l;
        }
    }
}
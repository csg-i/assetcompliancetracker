using System;
using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string SpecsWizard(this IUrlHelper helper, BuildSpecificationTypeConstant type, long? id = null)
        {
            var prefix = "Os";
            if (type == BuildSpecificationTypeConstant.Application)
                prefix = "App";
            return helper.RouteUrl("default", new { controller = $"{prefix}Specs", action = "Wizard", id });
        }

        public static string SpecsWizardDone(this IUrlHelper helper, BuildSpecificationTypeConstant type)
        {
            var prefix = "Os";
            if (type == BuildSpecificationTypeConstant.Application)
                prefix = "App";
            return helper.RouteUrl("default", new { controller = $"{prefix}Specs", action = "Done" });
        }
    }
}
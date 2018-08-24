using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string BuildSpecReport(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "Report", id });
        }
        public static string PartialBuildSpecReport(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "Specs", id});
        }

        public static string PartialPortReport(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "Ports", id});
        }
        public static string PartialAllPortsReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "Ports" });
        }

        public static string PartialByOwnerReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "ByOwner" });
        }

        public static string PartialAssignedNodes(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "AssignedNodes" });
        }
        public static string PartialNotReportingNodes(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "NotReportingNodes" });
        }
        public static string PartialNodesExludedByProduct(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "NodesExcludedByProduct" });
        }
        public static string PartialReviewData(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "ReviewData", id=specId, environmentId });
        }
        public static string PartialReviewDataDetails(this IUrlHelper helper, long specId, int environmentId, JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "ReviewDetails", id = specId, environmentId, resultType, shouldExist, portType });
        }
        public static string PartialReviewSuggestions(this IUrlHelper helper, long specId, int environmentId, JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "Suggestions", id = specId, environmentId, resultType, shouldExist, portType });
        }
        public static string PartialReviewDataErrorDetails(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new {controller = "BuildSpec", action = "ReviewErrorDetails", id = specId, environmentId});
        }

        public static string PartialReviewDataErrorMessages(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "ReviewErrorMessages", id = specId, environmentId });
        }

        public static string PartialReviewDataOsFailureDetails(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "ReviewOsFailureDetails", id = specId, environmentId });
        }

        public static string JsonAssignedNodeComplianceReports(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new { controller = "BuildSpec", action = "AssignedNodeIds", id = specId, environmentId });
        }

    }
}
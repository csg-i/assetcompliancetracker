using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.BuildSpec;
using act.core.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class BuildSpecController : PureMvcControllerBase
    {
        private readonly IBuildSpecificationFactory _buildSpecificationFactory;
        private readonly INodeFactory _nodeFactory;
        private readonly ISuggestionFactory _suggestionFactory;

        public BuildSpecController(IBuildSpecificationFactory buildSpecificationFactory, INodeFactory nodeFactory, ISuggestionFactory suggestionFactory, ILoggerFactory logger) : base(logger)
        {
            _buildSpecificationFactory = buildSpecificationFactory;
            _nodeFactory = nodeFactory;
            _suggestionFactory = suggestionFactory;
        }

        /// <summary>
        /// Used by the cookbook to lookup by FQDN the build spec for the compliance spec
        /// </summary>
        /// <param name="fqdn">FQDN of the machine requesting.</param>
        /// <returns>JSON meant to convert to Ruby Hash</returns>       
        [HttpGet]
        [AllowAnonymous]
        public ActionResult RetrieveFor(string fqdn)
        {
            SetNoCacheHeader();
            if (string.IsNullOrWhiteSpace(fqdn))
                return StatusCode(HttpStatusCode.BadRequest, "fqdn is required.");

            return Json(_buildSpecificationFactory.InspecForFqdn(fqdn));
        }
        
        /// <summary>
        /// Used by the cookbook to assign a node to a build spec
        /// </summary>
        /// <param name="id">Build Spec Id to assign to</param>
        /// <param name="fqdn">FQDN of the machine requesting.</param>
        /// <returns>Ok, NotFound, or BadRequest</returns>       
        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> AssignTo(long id, string fqdn)
        {
            SetNoCacheHeader();
            if (string.IsNullOrWhiteSpace(fqdn))
                return BadRequest("fqdn is required.");

            try
            {
                if(await _nodeFactory.AssignBuildSpecification(fqdn, id))
                    Logger.LogWarning($"{fqdn} was assigned to application spec {id} via an unauthenticated PUT.");

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ViewResult Report(long id)
        {
            var uri = GetUri();
            return View(new BuildSpecReport(id, uri.AbsoluteUri.Replace(uri.PathAndQuery, string.Empty)));
        }

        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        public async Task<PartialViewResult> Specs(long id)
        {
            return PartialView(await _buildSpecificationFactory.BuildSpecification(id));
        }

        [HttpPost]
        public async Task<PartialViewResult> ByOwner()
        {
            return PartialView(await _buildSpecificationFactory.ByOwnersReport());
        }

        [HttpPost]
        public async Task<PartialViewResult> Ports(long? id)
        {
            if (id.HasValue)
                return PartialView(await _buildSpecificationFactory.PortReport(id.Value));

            return PartialView(await _buildSpecificationFactory.AllPortsReport());
        }
        
        public async Task<PartialViewResult> AssignedNodes()
        {
            return PartialView(await _buildSpecificationFactory.AssignedNodesReport());
        }

        public async Task<PartialViewResult> NotReportingNodes()
        {
            return PartialView(await _buildSpecificationFactory.NotReportingNodesReport());
        }

        public async Task<PartialViewResult> NodesExcludedByProduct()
        {
            return PartialView(await _buildSpecificationFactory.NodesExludedByProductReport());
        }

        public async Task<PartialViewResult> ReviewData(long id, int environmentId)
        {
            return PartialView(await _buildSpecificationFactory.ReviewComplianceData(id, environmentId));
        }
        public async Task<PartialViewResult> ReviewErrorDetails(long id, int environmentId, string name, string code)
        {
            return PartialView(
                await _buildSpecificationFactory.ReviewComplianceDataDetailsForErrors(id, environmentId, name, code));
        }
        public async Task<PartialViewResult> ReviewErrorMessages(long id, int environmentId, string name, string code)
        {
            return PartialView(
                await _buildSpecificationFactory.ReviewComplianceDataErrorLongMessages(id, environmentId, name, code));
        }

        public async Task<PartialViewResult> ReviewOsFailureDetails(long id, int environmentId)
        {
            return PartialView(
                await _buildSpecificationFactory.ReviewComplianceDataDetailsForOsFailures(id, environmentId));
        }
        public async Task<PartialViewResult> ReviewDetails(long id, int environmentId, JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name)
        {
            return PartialView(
                await _buildSpecificationFactory.ReviewComplianceDataDetails(id, environmentId, resultType, shouldExist, portType, name));
        }

        public async Task<PartialViewResult> Suggestions(long id, JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name)
        {
            return PartialView(
                await _suggestionFactory.ComplianceDataSuggestions(id, resultType, shouldExist, portType, name));
        }

        public async Task<JsonResult> AssignedNodeIds(long id, int? environmentId)
        {
            var nodeIds = await _buildSpecificationFactory.AssignedNodeIds(id, environmentId.GetValueOrDefault());
            var urls = nodeIds.Select(p => Url.JsonComplianceReport(p));
            return Json(JsonEnvelope.Success(new {urls}));
        }

    }
}
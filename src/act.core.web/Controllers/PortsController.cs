using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.Ports;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class PortsController : PureMvcControllerBase
    {
        private readonly IPortFactory _portFactory;
        private readonly IJustificationFactory _justificationFactory;

        public PortsController(IPortFactory portFactory, IJustificationFactory justificationFactory, ILoggerFactory logger) : base(logger)
        {
            _portFactory = portFactory;
            _justificationFactory = justificationFactory;
        }

        [HttpPost]
        public PartialViewResult ForSpec(PlatformConstant id, long specId)
        {
            return PartialView(new PortsScreen(id, specId));
        }
        [HttpPost]

        public async Task<PartialViewResult> Get(PlatformConstant id, long specId)
        {
            var ports = await _portFactory.GetPorts(specId);
            return PartialView(new SimplePorts(id, ports.Select(p=>new ViewPort(id, specId, p))));
        }
        [HttpPost]

        public async Task<PartialViewResult> Edit(PlatformConstant id, long specId, long justificationId)
        {
            var port = await _portFactory.GetPort(justificationId);
            return PartialView(new EditPort(id, specId, port));
        }
        [HttpPost]

        public async Task<PartialViewResult> One(PlatformConstant id, long specId, long justificationId)
        {
            var port = await _portFactory.GetPort(justificationId);
            return PartialView(new ViewPort(id, specId, port));
        }
        [HttpPost]

        public PartialViewResult New(PlatformConstant id, long specId)
        {
            return PartialView(new NewPort(id, specId));
        }

        [HttpPost]
        public PartialViewResult Bulk(PlatformConstant id, long specId)
        {
            return PartialView(new BulkNewPorts(id, specId));
        }

        [HttpPost]
        public JsonResult Valdate(string justification, string ports, SimplePortDirectionTypeConstant? direction, PortTypeConstant? portType)
        {
            var errors = new Dictionary<string, string>();
            var ret = _portFactory.Validate(ports);

            if (!ret.IsValid)
                errors.Add("ports", $"The ports entered were not valid.  The valid configuration is {ret.Ports.ToPortString()}");
            if (!_justificationFactory.IsValidText(justification))
                errors.Add("justification","The Justifcation cannot be empty whitespace.");

            if(!direction.HasValue)
                errors.Add("direction", "You must choose a direction");

            if (!portType.HasValue)
                errors.Add("portType", "You must choose a type");

            if (errors.Count == 0)
                return Json(JsonEnvelope.Success());

            return Json(JsonEnvelope.Error(errors));
        }
        [HttpPost]
        public async Task<JsonResult> Delete(long id)
        {
            Logger.LogInformation($"{UserSecurity.SamAccountName} deleted ports and justification for justification id {id}");
            await _portFactory.DeletePort(id);
            await _justificationFactory.DeleteJustification(id);
            return Json(JsonEnvelope.Success());
        }
        [HttpPost]
        public async Task<JsonResult> Save(PlatformConstant id, long specId, long? justificationId, SimplePortDirectionTypeConstant direction, PortTypeConstant portType,
            string justification, string ports)
        {
            if(!_justificationFactory.IsValidText(justification))
                return Json(JsonEnvelope.Error("The Justifcation cannot be empty whitespace."));

            if (!justificationId.HasValue)
            {
                Logger.LogInformation($"{UserSecurity.SamAccountName} added a new justification for ports: {justification}");
                justificationId =
                    await _justificationFactory.AddJustification(JustificationTypeConstant.Port, specId, justification);
            }
            else
            {
                Logger.LogInformation($"{UserSecurity.SamAccountName} updated justification {justificationId} for ports: {justification}");
                await _justificationFactory.UpdateJustification(justificationId.Value, justification);
            }
            
            await _portFactory.AddOrUpdatePorts(specId, justificationId.Value, direction == SimplePortDirectionTypeConstant.PortListeningToOutsideTraffic, direction == SimplePortDirectionTypeConstant.SendingTrafficToOusidePort, portType, ports);
            Logger.LogInformation($"{UserSecurity.SamAccountName} added or updated ports for spec {specId} ports are: {ports}");
            return Json(JsonEnvelope.Success(new {url= Url.PartialOnePort(id, specId, justificationId.Value)}));
        }

    }
}
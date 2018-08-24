using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.Justifications;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class JustificationsController : PureMvcControllerBase
    {
        private readonly IJustificationFactory _justificationFactory;

        public JustificationsController(IJustificationFactory justificationFactory, ILoggerFactory logger) : base(logger)
        {
            _justificationFactory = justificationFactory;
        }


        [HttpPost]
        public async Task<PartialViewResult> ForSpec(JustificationTypeConstant id, long specId)
        {
            return PartialView(new Justifications(await _justificationFactory.GetJustifications(id, specId)));
        }
        [HttpPost]
        public async Task<PartialViewResult> Edit(long id)
        {
            var it = await _justificationFactory.GetJustification(id);
            return PartialView(new EditJustification(it.Id, it.Text));
        }

        [HttpPost]
        public async Task<PartialViewResult> Single(long id)
        {
            return PartialView(await _justificationFactory.GetJustification(id));
        }

        [HttpPost]
        public PartialViewResult New(JustificationTypeConstant id, long specId)
        {
            return PartialView(new NewJustification(specId, id));
        }

        [HttpPost]
        public async Task<JsonResult> Add(JustificationTypeConstant id, long specId, string text)
        {
            if (!_justificationFactory.IsValidText(text))
                return Json(JsonEnvelope.Error("The Justifcation cannot be empty whitespace."));

            var newid = await _justificationFactory.AddJustification(id, specId, text);
            Logger.LogInformation($"Justification {newid} added by {UserSecurity.SamAccountName} with text: {text}");

            return Json(JsonEnvelope.Success(new {  url = Url.PartialGetJustification(newid) }));
        }

        [HttpPost]
        public async Task<JsonResult> Delete(long id)
        {
            await _justificationFactory.DeleteJustification(id);
            Logger.LogInformation($"Justification {id} deleted by {UserSecurity.SamAccountName}.");

            return Json(JsonEnvelope.Success());
        }
        
        [HttpPost]
        public async Task<JsonResult> Update(long id, string text)
        {
            if (!_justificationFactory.IsValidText(text))
                return Json(JsonEnvelope.Error("The Justifcation cannot be empty whitespace."));

            await _justificationFactory.UpdateJustification(id, text);
            Logger.LogInformation($"Justification {id} updated by {UserSecurity.SamAccountName} with new text: {text}");

            return Json(JsonEnvelope.Success());
        }
        [HttpPost]
        public async Task<JsonResult> ChangeColor(long id, string color)
        {
            await _justificationFactory.ChangeColor(id, color);
            Logger.LogInformation($"Justification {id} color updated by {UserSecurity.SamAccountName} to {color}");

            return Json(JsonEnvelope.Success());
        }

    }
}
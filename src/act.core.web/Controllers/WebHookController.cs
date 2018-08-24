using System.IO;
using System.Threading.Tasks;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    public class WebHookController : PureMvcControllerBase
    {
        private readonly INotifier _notifier;
        public WebHookController(INotifier notifier, ILoggerFactory logger) : base(logger)
        {
            _notifier = notifier;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ObjectResult> Data()
        {
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                var data = reader.ReadToEnd();
                var result = await _notifier.NotifyComplianceFailure(data);

                return StatusCode(result.StatusCode, result.Exception?.Message);                
            }
        }
    }
}
using System.Threading.Tasks;
using act.core.web.Framework;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class EmployeesController : PureMvcControllerBase
    {
        private readonly IEmployeeFactory _data;

        public EmployeesController(IEmployeeFactory data, ILoggerFactory logger) : base(logger)
        {
            _data = data;
        }

        [HttpPost]
        public async Task<JsonResult> Search(string q)
        {
            if(q?.Length < 2)
                return Json(JsonEnvelope.Error("Waiting for input..."));

            var data = await _data.TypeAheadSearch(q);
            if (data.Length > 0)
                return Json(JsonEnvelope.Success(data));

            return Json(JsonEnvelope.Error("No results found"));
        }
        

    }
}
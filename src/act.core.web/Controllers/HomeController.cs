using System.Linq;
using act.core.web.Framework;
using act.core.web.Models.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    public class HomeController : PureMvcControllerBase
    {
        private readonly IConfiguration _config;

        public HomeController(ILoggerFactory logging, IConfiguration config) : base(logging)
        {
            _config = config;
        }
        
        public ViewResult Help()
        {
            var links = _config.GetSection("HelpLinks").GetChildren().ToArray();
            var help = new Help();
            foreach (var link in links)
            {
                var helpLink = new HelpLink();
                link.Bind(helpLink);
                help.Add(helpLink);
            }
            
            return View(help);
        }

        [AllowAnonymous]
        [HttpGet]
        public ViewResult Index()
        {
            if (IsLoggedIn)
                return View(new Welcome(UserSecurity));

            return View(new Home());
        }

        [HttpGet]
        public JsonResult WhoAmI()
        {
            return Json(JsonEnvelope.Success(UserSecurity));
        }
    }
}

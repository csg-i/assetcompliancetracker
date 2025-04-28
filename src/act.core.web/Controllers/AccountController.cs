using System.Threading.Tasks;
using act.core.web.Extensions;
using act.core.web.Framework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    public class AccountController : PureMvcControllerBase
    {
        public AccountController(ILoggerFactory logger) : base(logger)
        {
        }

        public IActionResult SignIn(string returnUrl)
        {
            Logger.LogDebug("Explicit sign-in occurred.");
            SetNoCacheHeader();
            return LocalRedirect(returnUrl ?? Url.Home());
        }

        public override SignOutResult SignOut()
        {
            Logger.LogDebug("Explicit sign-out occurred.");
            if (HttpContext.Request.Cookies["ACT"] != null)
                Response.Cookies.Delete("ACT");

            SetNoCacheHeader();
            HttpContext.SignOutAsync(WsFederationDefaults.AuthenticationScheme).Wait();
            return new SignOutResult(WsFederationDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Home()
            });
        }
    }
}
using System.Threading.Tasks;
using act.core.web.Extensions;
using act.core.web.Framework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    public class AccountController : PureMvcControllerBase
    {

        public AccountController(ILoggerFactory logger) : base(logger)
        {
        }

        public RedirectResult SignIn(string returnUrl)
        {
            Logger.LogDebug("Explicit sign-in occurred.");
            SetNoCacheHeader();
            return Redirect(returnUrl ?? Url.Home());
        }

        public async Task SignOut()
        {
            Logger.LogDebug("Explicit sign-out occurred.");
            if (HttpContext.Request.Cookies["ACT"] != null)
                Response.Cookies.Delete("ACT");

            SetNoCacheHeader();
            await HttpContext.SignOutAsync(WsFederationDefaults.AuthenticationScheme);
        }
    }
}
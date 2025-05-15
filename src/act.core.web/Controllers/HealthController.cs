using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Framework;
using act.core.web.Models.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace act.core.web.Controllers
{
    [AllowAnonymous]
    public class HealthController : PureMvcControllerBase
    {
        private readonly ActDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public HealthController(ActDbContext dbContext, ILoggerFactory logger, IConfiguration configuration) : base(logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        // GET: Health
        [HttpGet]
        public async Task<ViewResult> Index()
        {
            Logger.LogTrace("Health Controller Executing Disgnostics");
            bool connected;
            try
            {
                connected = await _dbContext.BuildSpecifications.AnyAsync();
            }
            catch (MySqlException ex)
            {
                Logger.LogWarning(ex, "Error Connecting to DB");
                connected = false;
            }

           
          
            if (IsLoggedIn)
            {
                var cs = _dbContext.Database.GetDbConnection().ConnectionString;
                var split = cs.Split(';').ToArray();
                if (split.Length > 1)
                {
                    var noSecret = split.Where(p => !p.ToLower().StartsWith("password="));
                    cs = string.Join(";", noSecret);
                }

                Func<IEnumerable<IConfigurationSection>, IEnumerable < string >> recurse = null;
                recurse = sections =>
                {
                    return sections.Where(p => !p.Key.ToLower().Contains("password") && !p.Key.ToLower().Contains("token") && p.Exists() && (p.Value == null || (!p.Value.ToLower().Contains("password") && !p.Value.ToLower().Contains("token")))).Select(p =>
                    {
                        if (p.Value != null)
                            return $"{p.Key}:{p.Value}";

                        var more = recurse.Invoke(p.GetChildren()).ToArray();
                        return $"{p.Key}:@({string.Join(",\n", more)})";
                     
                    });
                };
                var settings = recurse.Invoke(_configuration.GetChildren()).ToArray();

                return View(new DetailedHealthCheck(UserSecurity, connected, settings, cs));
            }

            return View(new SimpleHealthCheck(connected));
        }

        // GET: Error/302
        [HttpGet]
        public ViewResult Error(string id)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature != null)
            {
                string routeWhereExceptionOccurred = exceptionFeature.Path;
                
                Exception exceptionThatOccurred = exceptionFeature.Error;

                Logger.LogWarning(exceptionThatOccurred, $"Error Page Reached with statuscode {id} on route {routeWhereExceptionOccurred}.");

            }
            else if(id != "404")
            {
                Logger.LogWarning($"Error Page Reached with statuscode {id}.");
            }
            return View(new ErrorPage(id, Activity.Current?.Id ?? HttpContext.TraceIdentifier));
        }

        public IActionResult TestError()
        {
            throw new Exception("Testing Errors");
        }

    }
}
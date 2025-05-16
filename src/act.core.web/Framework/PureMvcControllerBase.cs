using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace act.core.web.Framework
{
    [AllowAnonymous]
    public abstract class PureMvcControllerBase : Controller
    {
        class UserSecurityInternal : IUserSecurity
        {
            public string UserName { get; set; }
            public string SamAccountName { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public long EmployeeId { get; set; }



            public UserSecurityInternal(string userName, string samAccountName, string lastName, string firstName, string email, long employeeId)
            {
                SamAccountName = samAccountName;
                LastName = lastName;
                FirstName = firstName;
                Email = email;
                EmployeeId = employeeId;
                UserName = userName;
            }
        }

        private UserSecurityInternal _userSecurity;
        protected readonly ILogger Logger;
        protected PureMvcControllerBase(ILoggerFactory logging)
        {
            TempData = new NullTempDataDictionary();
            Logger = logging.CreateLogger(GetType());
        }

        public Uri GetUri()
        {
            const string unknownHostName = "UNKNOWN-HOST";
            var request = HttpContext.Request;
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (true == string.IsNullOrWhiteSpace(request.Scheme))
            {
                throw new ArgumentException("Http request Scheme is not specified");
            }

            string hostName = request.Host.HasValue ? request.Host.ToString() : unknownHostName;
            
            var builder = new StringBuilder();

            builder.Append(request.Scheme)
                .Append("://")
                .Append(hostName);

            if (true == request.Path.HasValue)
            {
                builder.Append(request.Path.Value);
            }

            if (true == request.QueryString.HasValue)
            {
                builder.Append(request.QueryString);
            }

            return new Uri(builder.ToString());
        }
        public bool IsLoggedIn => HttpContext.User.Identity.IsAuthenticated;

        public IUserSecurity UserSecurity
        {
            get
            {
//                if (!IsLoggedIn)
//                   return null;

                if (_userSecurity == null)
                {
//                    var clp = HttpContext.User;
//                    if (clp != null)
//                    {
                        _userSecurity = new UserSecurityInternal(
                            "Priyanka Kovermanne Somashekhara",
                            "kovpri01",
                            "Kovermanne Somashekhara",
                            "Priyanka",
                            "Priyanka.KovermanneSomashekhara@csgi/com",
                             81896
                        );
//                    }
                }
                return _userSecurity;
            }

        }
        public void AddHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var h in headers)
            {
                foreach (var v in h.Value)
                {
                    Response.Headers.Add(h.Key, v);
                }
            }
        }
        public void SetNoCacheHeader()
        {
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Pragma", "no-cache");
        }

        /// <summary>
        /// function to derive the name from the model class;
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string GetName(object model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Type t = model.GetType();
            string name = t.Name;
            if (t.IsGenericType)
            {
                name = name.Substring(0, name.IndexOf("`", StringComparison.Ordinal));
                var gentype = t.GetGenericArguments();
                name = gentype.Aggregate(name, (current, type) => $"{current}Of{type.Name}");
            }
            return name;
        }
        [NonAction]
        public override ViewResult View(string viewName, object model)
        {

            ViewData.Model = model;
            if (viewName == null && model != null)
            {
                viewName = $"{GetName(model)}View";
            }
            else if (model == null)
            {
                Logger.LogWarning($"View called without a model and view name {viewName}");
            }
            return new ViewResult
            {
                ViewName = viewName,
                ViewData = ViewData
            };
        }
        [NonAction]
        public override PartialViewResult PartialView(string viewName, object model)
        {
            ViewData.Model = model;
            if (viewName == null && model != null)
            {
                viewName = $"{GetName(model)}Partial";
            }
            else if (model == null)
            {
                Logger.LogWarning($"ParitalView called without a model and view name {viewName}.");
            }
            return new PartialViewResult
            {
                ViewName = viewName,
                ViewData = ViewData
            };
        }

        public ObjectResult StatusCode(HttpStatusCode statusCode, object value=null)
        {
            return base.StatusCode((int)statusCode, value);
        }
    }
}
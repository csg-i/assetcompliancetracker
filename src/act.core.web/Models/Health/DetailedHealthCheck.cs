using System.Collections.Generic;
using act.core.web.Framework;
using Newtonsoft.Json;

namespace act.core.web.Models.Health
{
    public class DetailedHealthCheck:SimpleHealthCheck
    {
        public IEnumerable<string> Configuration { get; }
        public string ContextConnectionStringInfo { get; }
        public string LoggedInUser { get; }

        public DetailedHealthCheck(IUserSecurity loggedInUser, bool dbConnected, string[] configuration, string contextConnectionStringInfo ) :base(dbConnected)
        {
            Configuration = configuration;
            ContextConnectionStringInfo = contextConnectionStringInfo;
            LoggedInUser = JsonConvert.SerializeObject(loggedInUser);
        }
    }
}
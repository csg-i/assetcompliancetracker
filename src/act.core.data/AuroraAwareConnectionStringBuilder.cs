using Microsoft.Extensions.Configuration;

namespace act.core.data
{
    public class AuroraAwareConnectionStringBuilder
    {
        private readonly IConfiguration _configuration;

        public string AuroraServer => _configuration.GetValue<string>("AURORA_SERVER");
        public string AuroraDb => _configuration.GetValue<string>("AURORA_DB");
        public string AuroraUser => _configuration.GetValue<string>("AURORA_USER");
        public string AuroraPassword => _configuration.GetValue<string>("AURORA_PASSWORD");

        public AuroraAwareConnectionStringBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString(string fallbackConnectionStringName)
        {
            if (string.IsNullOrWhiteSpace(AuroraDb))
                return _configuration.GetConnectionString(fallbackConnectionStringName);
            return $"Server={AuroraServer};Database={AuroraDb};User={AuroraUser};Password={AuroraPassword};";
        }
    }
}

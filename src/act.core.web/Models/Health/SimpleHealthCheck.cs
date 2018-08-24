using System.Diagnostics;

namespace act.core.web.Models.Health
{
    public class SimpleHealthCheck
    {
        public bool DbConnected { get; }

        public string AssemblyVersion { get; }
        public SimpleHealthCheck(bool dbConnected)
        {
            DbConnected = dbConnected;
            AssemblyVersion = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).ProductVersion;
        }
    }
}
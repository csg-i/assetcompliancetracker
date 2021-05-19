using System.Collections.Generic;
using System.Security.AccessControl;
using act.core.data;
using act.core.web.Extensions;

namespace act.core.web.Models.OsSpecs
{
    public class OsSpecInformation : ISpecInformation
    {
        public long? Id { get; set; }
        public string Name { get; set; }

        public PlatformConstant Platform { get; set; }

        public string OsName { get; set; }

        public string OsVersion { get; set; }

        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }

        public string Email { get; set; }
        public bool IncludeRemedyEmailList { get; set; }
        public IDictionary<string, string> Validate()
        {
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 256)
                errors.Add("name", "The Build Specification Name is required with a max length of 256 characters.");
            if (string.IsNullOrWhiteSpace(OsName) || OsName.Length > 256)
                errors.Add("osname", "The  OS Name is required with a max length of 256 characters.");
            if (string.IsNullOrWhiteSpace(OsVersion) || OsVersion.Length > 256)
                errors.Add("osversion", "The OS Version is required with a max length of 32 characters.");
            if (OwnerId == null)
                errors.Add("ownername", "The Owner is required.");
            if (Email != null && !Email.IsValidEmail())
                errors.Add("email", "The Email must be a valid address");
            return errors;
        }
    }
}
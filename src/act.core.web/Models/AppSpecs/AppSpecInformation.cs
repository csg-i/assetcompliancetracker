using System.Collections.Generic;
using act.core.data;
using act.core.web.Extensions;

namespace act.core.web.Models.AppSpecs
{
    public class AppSpecInformation : ISpecInformation
    {
        public AppSpecInformation()
        {
            Platform = PlatformConstant.Linux.ToString().ToLower();
        }
        public long? Id { get; set; }
        public string Name { get; set; }

        public long? OsSpecId { get; set; }
        public string OsSpecName { get; set; }
        public string OwnerName { get; set; }
        public long? OwnerId { get; set; }
        public string Email { get; set; }
        public bool IncludeRemedyEmailList { get; set; }
        public string WikiLink { get; set; }
        public string Overview { get; set; }

        public string Platform { get; set; }

        public bool? RunningCoreOs { get; set; }
        public IDictionary<string, string> Validate()
        {
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 256)
                errors.Add("name", "The Build Specification Name is required with a max length of 256 characters.");
            if (OsSpecId == null || OsSpecId <= 0)
                errors.Add("osspecname", "The OS Specification cannot be blank.");
            if (OwnerId == null)
                errors.Add("ownername", "The Owner is required.");
            if (WikiLink != null && WikiLink.Length > 256)
                errors.Add("wikilink", "The Wiki Link has a max length of 256 characters.");
            if (Email != null && !Email.IsValidEmail())
                errors.Add("email", "The Email must be a valid email address");

            return errors;
        }

    }
}
using System.Collections.Generic;

namespace act.core.web.Models
{
    public interface ISpecInformation
    {
        long? Id { get; set; }
        string Name { get; set; }

        string OwnerName { get; set; }

        long? OwnerId { get; set; }

        string Email { get; set; }
        IDictionary<string, string> Validate();
    }
}
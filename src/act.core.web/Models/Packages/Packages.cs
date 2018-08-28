using System.Collections.Generic;

namespace act.core.web.Models.Packages
{
    public class Packages : List<Package>
    {
        public Packages(IEnumerable<Package> packages) : base(packages)
        {
            
        }

        public bool IsEmpty => this.Count == 0;
    }
}
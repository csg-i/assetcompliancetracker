using System.Collections.Generic;

namespace act.core.web.Models.BuildSpec
{
    public class SpecByOwners : List<SpecByOwner>
    {
        public SpecByOwners(IEnumerable<SpecByOwner> collection) : base(collection)
        {
            
        }        
    }
}
using System.Collections.Generic;

namespace act.core.web.Models.Justifications
{
    public class Justifications:List<Justification>
    {
        public Justifications(IEnumerable<Justification> collection) : base(collection)
        {
            
        }
        public bool IsEmpty => this.Count == 0;
    }
}
using System.Collections.Generic;

namespace act.core.data
{
    public class Product
    {
        public Product()
        {
            Nodes = new HashSet<Node>();
        }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool ExludeFromReports { get; set; }

        public ICollection<Node> Nodes { get; set; }
    }
}
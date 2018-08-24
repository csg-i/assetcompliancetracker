using System.Collections.Generic;

namespace act.core.data
{
    public class Function
    {
        public Function()
        {
            Nodes = new HashSet<Node>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Node> Nodes { get; set; }
    }
}
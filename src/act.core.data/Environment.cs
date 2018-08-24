using System.Collections.Generic;

namespace act.core.data
{
    public class Environment
    {
        public Environment()
        {
            Nodes = new HashSet<Node>();
            SoftwareComponentEnvironments = new HashSet<SoftwareComponentEnvironment>();
        }

        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string ChefAutomateUrl { get; set; }
        
        public string ChefAutomateToken { get; set; }
        
        public string ChefAutomateOrg { get; set; }
        
        public string Color { get; set; }
        
        public ICollection<Node> Nodes { get; set; }
        
        public ICollection<SoftwareComponentEnvironment> SoftwareComponentEnvironments { get; set; }
    }
}
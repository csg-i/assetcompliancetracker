namespace act.core.web.Models.Dashboard
{
    public class Stat
    {
        public string Name { get; }
        public int SpecCount { get; }
        public int NodeCount { get; }

        public Stat(string name, int specCount, int nodeCount)
        {
            Name = name;
            SpecCount = specCount;
            NodeCount = nodeCount;
        }
    }
}
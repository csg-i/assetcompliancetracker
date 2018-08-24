using System.Collections.Generic;

namespace act.core.web.Models.BuildSpec
{
    public class ReviewErrorDetails:List<string>
    {
        public string Name { get; }
        public string Code { get; }

        public ReviewErrorDetails(string name, string code, IEnumerable<string> collection) : base(collection)
        {
            Name = name;
            Code = code;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class FailedTest
    {
        public JustificationTypeConstant ResultType { get; set; }
        public PortTypeConstant? PortType{ get; set; }
        public string Name{ get; set; }
        public bool ShouldExist { get; set; }
        public int Count { get; set; }
    }

    public class Error
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int Count { get; set; }
    }
    public class ReviewData:List<FailedTest>
    {
        public long Id { get; }
        public string Name { get; }
        public int EnvironmentId { get; }
        public string EnvironmentName { get; }
        public int CountOfOsFailures { get; }

        public IEnumerable<Error> Errors { get; }

        public ReviewData(long id, string name, int environmentId, string environmentName, int countOfOsFailures, IEnumerable<Error> errors, IEnumerable<FailedTest> results):base(results.OrderBy(p=>p.ResultType).ThenBy(p=>p.ShouldExist).ThenBy(p=>p.PortType).ThenByDescending(p=>p.Count))
        {
            Id = id;
            Name = name;
            EnvironmentId = environmentId;
            EnvironmentName = environmentName;
            Errors = errors;
            CountOfOsFailures = countOfOsFailures;
        }
        
    }
}
using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class ReviewDataItems : List<ReviewDataItem>
    {
        public JustificationTypeConstant ResultType { get; }
        public bool ShouldExist { get; }
        public PortTypeConstant? PortType { get; }
        public string Name { get; }

        public bool OsFailures { get; private set; }
        public bool Errors { get; private set; }
        private ReviewDataItems(IEnumerable<ReviewDataItem> collection):base(collection)
        {

        }
        public static ReviewDataItems ForErrors(IEnumerable<ReviewDataItem> collection)
        {
            return new ReviewDataItems(collection)
            {
                Errors = true
            };
        }
        public static ReviewDataItems ForOsFailures(IEnumerable<ReviewDataItem> collection)
        {
            return new ReviewDataItems(collection)
            {
                OsFailures = true
            };
        }

        public ReviewDataItems(JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name, IEnumerable<ReviewDataItem> collection) : base(collection)
        {
            ResultType = resultType;
            ShouldExist = shouldExist;
            PortType = portType;
            Name = name;
        }
    }
}
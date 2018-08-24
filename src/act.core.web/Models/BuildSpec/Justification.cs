using System.Collections.Generic;
using System.Linq;
using act.core.data;
using Microsoft.AspNetCore.Html;

namespace act.core.web.Models.BuildSpec
{
    public class Justification
    {
        public JustificationTypeConstant JustificationType { get; }
        public string Justifier { get; }
        public BuildSpecificationTypeConstant FromSpecType { get; }
        public HtmlString Text { get; }
        public IOrderedEnumerable<Software> Software { get; }

        public Justification(JustificationTypeConstant justificationType, string text, string justifier, BuildSpecificationTypeConstant fromSpecType, IEnumerable<Software> software)
        {
            JustificationType = justificationType;
            Justifier = justifier;
            FromSpecType = fromSpecType;
            Text = new HtmlString((text??string.Empty).Replace("\n","<br/>"));
            Software = (software ?? Enumerable.Empty<Software>()).OrderBy(p=>p.Name);
        }
    }
}
using act.core.data;
using Microsoft.AspNetCore.Html;

namespace act.core.web.Models.Justifications
{
    public class Justification
    {
        private readonly JustificationTypeConstant _justificationType;

        public Justification(long id, string text, JustificationTypeConstant justificationType, string color)
        {
            Id = id;
            Text = text;
            _justificationType = justificationType;
            Color = color;
        }

        public long Id { get;  }
        public string Text { get;  }
        public string Color { get; }   
        public string FriendlyNamePlural => $"{_justificationType}s";
        public string FriendlyNameLowerPlural => $"{_justificationType}s".ToLower();

        public HtmlString HtmlText => new HtmlString((Text ?? string.Empty).Replace("\n", "<br/>"));
        
    }
}
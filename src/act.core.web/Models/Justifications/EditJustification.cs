namespace act.core.web.Models.Justifications
{
    public class EditJustification
    {
        public long Id { get; }
        public string Text { get; }

        public EditJustification(long id, string text)
        {
            Id = id;
            Text = text;
        }
    }
}
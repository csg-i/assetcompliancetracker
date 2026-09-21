namespace act.core.etl
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string From { get; set; }

        /// <summary>
        /// When set, PCI Class C digest mail is sent here instead of the GAM owner.
        /// Leave empty to send to the real owner.
        /// </summary>
        public string TestRecipient { get; set; }
    }
}
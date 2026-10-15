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

        /// <summary>
        /// CC on PCI Class C remediation mail. Previous recipients stay on To as escalation moves up.
        /// </summary>
        public string EscalationCc { get; set; } = "list-glb-agilearchitecture@csgi.com";
    }
}
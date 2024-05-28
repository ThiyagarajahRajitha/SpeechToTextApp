namespace SpeechAPI.Models
{
    public class Notification
    {
        public string Subject { get; set; }
        public IEnumerable<string> TO { get; set; }
        public IEnumerable<string> Bcc { get; set; }
        public string HtmlBody { get; set; }
        public string AttachmentName { get; set; }


        public Notification(string subject, IEnumerable<string> to, string htmlBody)
        {
            Subject = subject;
            TO = to;
            HtmlBody = htmlBody;
            Bcc = null;
        }
        public Notification(string subject, IEnumerable<string> to, IEnumerable<string> bcc, string htmlBody, string attachmentName)
        {
            Subject = subject;
            TO = to;
            Bcc = bcc;
            HtmlBody = htmlBody;
            AttachmentName = attachmentName;
        }
        public Notification(string subject, IEnumerable<string> to, IEnumerable<string> bcc, string htmlBody, string attachmentName, byte[] pdfBytes)
        {
            Subject = subject;
            TO = to;
            Bcc = bcc;
            HtmlBody = htmlBody;
            AttachmentName = attachmentName;
            pdfBytes = pdfBytes;
        }
    }
}

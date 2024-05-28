using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SpeechAPI.Models;

namespace SpeechAPI.Services
{
    public class GmailSendingService
    {
        private GmailService GmailService { get; set; }
        string[] Scopes = { GmailService.Scope.MailGoogleCom };
        string ApplicationName = "Speech To Text Application";
        string SenderEmail = "thiyagarajahrajitha@gmail.com";

        public GmailSendingService()
        {
            UserCredential credential;
            using (var stream = new FileStream("GmailKey.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "gmail-dotnet-credentials.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            // Create Gmail API service.
            GmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
        public async Task SendMail(Notification message, byte[] pdfd)
        {
            try
            {
                Message InternalMessage = GetSendMessage(message, pdfd);
                await GmailService.Users.Messages.Send(InternalMessage, "me").ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in sending email:", ex.Message);
            }

        }

        private Message GetSendMessage(Notification gmailMessage, byte[] pdfBytes)
        {
            //Validate input parameters(optional)
            if (pdfBytes == null || string.IsNullOrEmpty(gmailMessage.AttachmentName))
            {
                throw new ArgumentException("PDF bytes or attachment name cannot be null or empty.");
            }

            // Build the plain text with headers
            string plainText = $"From:{ApplicationName}<{SenderEmail}>\r\n" +
                                $"To:{GenerateReceipents(gmailMessage.TO)}\r\n" +
                                $"Bcc:{GenerateReceipents(gmailMessage.Bcc)}\r\n" +
                                $"Subject:{gmailMessage.Subject}\r\n"+
                                "Content-Type: multipart/mixed; boundary=myboundary\r\n\r\n";

            // Create a message object
            Message message = new Message();

            // 1. Set message body (optional)
            if (!string.IsNullOrEmpty(gmailMessage.HtmlBody))
            {
                plainText += "--myboundary\r\n" +
                             "Content-Type: text/html; charset=us-ascii\r\n\r\n" +
                             $"{gmailMessage.HtmlBody}\r\n";
            }

            // 2. Add attachment part
            plainText += "--myboundary\r\n" +
                         "Content-Type: application/pdf; name=\"" + gmailMessage.AttachmentName + ".pdf\"\r\n" +
                         "Content-Transfer-Encoding: base64\r\n" +
                         "Content-Disposition: attachment; filename=\"" + gmailMessage.AttachmentName + ".pdf\"\r\n\r\n";
            plainText += Convert.ToBase64String(pdfBytes) + "\r\n";

            //Set the raw encoded message
            message.Raw = Encode(plainText);

            return message;
        }

        private string GenerateReceipents(IEnumerable<string> receipents)
        {
            return receipents == null ? string.Empty : string.Join(",", receipents);
        }
        private string Encode(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}


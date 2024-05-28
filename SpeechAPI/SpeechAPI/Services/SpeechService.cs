using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeechAPI.Interfaces;
using SpeechAPI.Models;
using SpeechAPI.Utils.ConfigOptions;
using System.Text;
using Document = iTextSharp.text.Document;

namespace SpeechAPI.Services
{
    public class SpeechService : ISpeechService
    {
        private readonly GCSConfigOptions _options;
        private readonly ILogger<SpeechService> _logger;
        private readonly GoogleCredential _googleCredential;
        private readonly GmailSendingService _gmailSendingService = new GmailSendingService();

        string transcriptionSubject = "Your transcribed text is here";
        string transcriptionBody = """
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Transcribing is completed!</title>
            </head>
            <body>
                <table width="100%" bgcolor="#f0f0f0">
                    <tr>
                        <td align="center">
                            <table width="600" bgcolor="#ffffff" style="border-radius: 10px; box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);">
                                <tr>
                                    <td style="padding: 20px;">
                                        <h1>Transcription is Ready!</h1>
                                        <p>Hello!, </p>
                                        <p>We are writing to inform you that the transcribed text for the given audio has been completed by the system.</p>
                                        
                                        <p>Best regards,</p>
                                        <p>Transcribing Team, Peoples Bank</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
        public SpeechService(IOptions<GCSConfigOptions> options, ILogger<SpeechService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _googleCredential = GoogleCredential.FromFile(_options.GCPStorageAuthFile);

        }
        public async Task<string> UploadFile(IFormFile fileToUpload)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileToUpload.CopyToAsync(memoryStream);
                    using (var storageClient = StorageClient.Create(_googleCredential))
                    {
                        var uploadFile = await storageClient.UploadObjectAsync(_options.GoogleCloudStorageBucketName, fileToUpload.FileName.ToString(), fileToUpload.ContentType, memoryStream);
                        //string gsUri = $"gs://{_options.GoogleCloudStorageBucketName}/ABC.wav";
                        string gsUri = $"gs://"+_options.GoogleCloudStorageBucketName+"/"+fileToUpload.FileName;
                        //return uploadFile.MediaLink.ToString();
                        return gsUri;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;

            }
        }
        public async Task<string> GetTranscription(string gcsUri)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(300);
                var content = new StringContent(gcsUri);
                var apiUrl = "http://127.0.0.1:8000/transcribe/";
                apiUrl = string.Concat(apiUrl, "?gcs_uri=", gcsUri);
                var response = await client.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject jObject = JsonConvert.DeserializeObject<JObject>(responseString);
                    string combinedTranscript = (string)jObject["combinedTranscript"];

                    if (combinedTranscript != null)
                    {
                        IEnumerable<string> sender = new string[] { "rajithiya93@gmail.com" };
                        string body = string.Format(transcriptionBody, "Transcribed Text Topic");
                        string attachmentName = "Transcription";
                        Notification notification = new Notification(transcriptionSubject, sender, null, body, attachmentName);

                        // Directly use the combinedTranscript for Base64 encoding (assuming it's text)
                        string base64EncodedPdfData = Convert.ToBase64String(Encoding.UTF8.GetBytes(combinedTranscript));

                        // Use a library to create a PDF from the Base64 encoded data (replace with your chosen library)
                        byte[] pdfBytes = CreatePdfFromBase64(base64EncodedPdfData);



                        //using (FileStream fs = new FileStream("C:\\Users\\Rajitha\\Downloads\\" + attachmentName + ".pdf", FileMode.Create, FileAccess.Write))
                        //{
                        //    fs.Write(pdfBytes, 0, pdfBytes.Length);
                        //}
                        await _gmailSendingService.SendMail(notification, pdfBytes);
                        //Console.WriteLine("PDF downloaded successfully!");

                        return combinedTranscript;
                    }
                    else
                    {
                        // Handle unexpected response format (optional)
                        Console.WriteLine("Unexpected response format.");
                        return response.Content.ToString(); // Or throw an exception
                    }
                }
                else
                {
                    Console.WriteLine("Error in transcribing");
                    return response.StatusCode.ToString();
                }
            }
        }

        private byte[] CreatePdfFromBase64(string base64EncodedPdfData)
        {
            // Decode the Base64 string
            byte[] decodedBytes = Convert.FromBase64String(base64EncodedPdfData);

            // Create a new Document object with desired page size (adjust if needed)
            Document document = new Document(PageSize.A4);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Create a PDF writer instance associated with the memory stream
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                // Open the document for writing content
                document.Open();

                // ... (Rest of your code to add content using iTextSharp)
                string decodedText = Encoding.UTF8.GetString(decodedBytes);
                BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED, false);
                int fontSize = 12;
                int justification = Element.ALIGN_JUSTIFIED; // Or Element.ALIGN_LEFT for left-aligned
                // Create a new Paragraph object with the text
                Paragraph paragraph = new Paragraph(decodedText);
                paragraph.Alignment = justification;
                //paragraph.Font=font;


                document.Add(paragraph);
                // Close the document
                document.Close();
                writer.Close();

                // Return the PDF document as a byte array
                return memoryStream.ToArray();
            }
        }
    }
}

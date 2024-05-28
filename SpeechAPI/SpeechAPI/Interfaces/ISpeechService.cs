using SpeechAPI.Models;

namespace SpeechAPI.Interfaces
{
    public interface ISpeechService
    {
        Task<string> GetTranscription(string gcsUri);
        Task<string> UploadFile(IFormFile fileToUpload);
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using SpeechAPI.Interfaces;
using SpeechAPI.Utils.ConfigOptions;
using System.Security.AccessControl;

namespace SpeechAPI.Services
{
    public class GCSUploaderService
    {
        //private readonly GCSConfigOptions _options;
        //private readonly ILogger<GCSUploaderService> _logger;
        //private readonly GoogleCredential _googleCredential;

        //public GCSUploaderService(IOptions<GCSConfigOptions> options, ILogger<GCSUploaderService> logger)
        //{
        //    _options = options.Value;
        //    _logger= logger;
        //    _googleCredential = GoogleCredential.FromFile(_options.GCPStorageAuthFile);

        //}
        //public async Task<string> UploadFile(IFormFile fileToUpload, string fileNameToSave)
        //{
        //    try
        //    {
        //        using(var memoryStream = new MemoryStream())
        //        {
        //            await fileToUpload.CopyToAsync(memoryStream);
        //            using(var storageClient = StorageClient.Create(_googleCredential)) 
        //            {
        //                var uploadFile = await storageClient.UploadObjectAsync(_options.GoogleCloudStorageBucketName, fileNameToSave, fileToUpload.ContentType, memoryStream);
        //                string gsUri = $"gs://{_options.GoogleCloudStorageBucketName}/{fileNameToSave}.wav";
        //                //return uploadFile.MediaLink.ToString();
        //                return gsUri;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw;
               
        //    }  
        //}
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using SpeechAPI.Interfaces;

namespace SpeechAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SpeechController : ControllerBase
    {
        private ISpeechService _speechService;
        //private readonly GCSUploaderService gCSUploader = new GCSUploaderService();
        //private IGCSUploaderService _gcsUploaderService;
        private readonly ITempDataDictionary _tempData;

        public SpeechController(ISpeechService speechService
            //,ITempDataDictionary tempData
            )
        {
            _speechService = speechService;
            //_gcsUploaderService = gcsUploaderService;
            //_tempData = tempData;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetTranscription(string gsUtilUri)
        {
            //string gsUtilUri = _tempData["gsUtilUri"] as string;
            //SpeechDTO speechTransciption = new SpeechDTO();
            try
            {
                string speechTranscription = "";
                if (gsUtilUri != null)

                {
                    //if (gsUtilUri.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    gsUtilUri = gsUtilUri.Substring(0, gsUtilUri.Length - 4);
                    //}
                    //else
                    //{
                    //    gsUtilUri = gsUtilUri;
                    //}
                    speechTranscription = await _speechService.GetTranscription(gsUtilUri);
                    //gs://speechbucket2/Dananjaya Hettiarachchi - World Champion of Public Speaking 2014 - Full Speech.wav
                }
                else
                {
                    // Handle the case where URI is not available (e.g., upload failed)
                    speechTranscription = await _speechService.GetTranscription("gs://speechbucket2/Dananjaya Hettiarachchi - World Champion of Public Speaking 2014 - Full Speech.wav");
                }
                string transcriptionText = speechTranscription.ToString();
                // return Ok(request.Result.ToString());
                //return Ok(new { transcription = transcriptionText });
                return Ok(JsonConvert.SerializeObject(new { transcription = transcriptionText }));
                //return Ok(speechTranscription);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
        }

        [Route("UploadFile")]
        [DisableRequestSizeLimit]
        [HttpPost]
        //public async Task<ActionResult> UploadFile(IFormFile fileToUpload)
        //{
        //    //var request = _gcsUploaderService.UploadFile(fileToUpload, fileNameToSave);
        //    var request = _speechService.UploadFile(fileToUpload);
        //    //_tempData["gsUtilUri"] = request.Result;
        //    return Ok(request.Result);

        //}
        public IActionResult UploadFile(IFormFile file)
        {
            try
            {
                var fileToUpload = file;
                if (fileToUpload == null)
                {
                    return BadRequest("No file uploaded");
                }
                var request = _speechService.UploadFile(fileToUpload);
                Console.WriteLine(request.Result.ToString());
                string gsUtilUriText = request.Result.ToString();
                // return Ok(request.Result.ToString());
                //return Ok(new { transcription = transcriptionText });
                return Ok(JsonConvert.SerializeObject(new { gsUtilUri = gsUtilUriText }));
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}

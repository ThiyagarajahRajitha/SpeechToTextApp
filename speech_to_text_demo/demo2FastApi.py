from fastapi import FastAPI, HTTPException, Query
from google.oauth2 import service_account
from google.cloud import speech, storage
from typing import Optional
import tempfile
import wave
import os

app = FastAPI()
def get_audio_info(gcs_uri: str) -> Optional[dict]:
    temp_file_path = None  # Initialize temp_file_path here
    try:
        # Extract the bucket name and file path from the GCS URI
        gcs_path = gcs_uri.replace("gs://", "")
        bucket_name, file_path = gcs_path.split("/", 1)
        
        print(f"Bucket: {bucket_name}, File Path: {file_path}")

        # Initialize a GCS client
        storage_client = storage.Client()
        bucket = storage_client.bucket(bucket_name)
        blob = bucket.blob(file_path)
        
        # Download the file to a temporary location
        # temp_file_path = f"/tmp/{file_path.split('/')[-1]}"
        # print(f"Downloading file to {temp_file_path}")
        # blob.download_to_filename(temp_file_path)

        # Create a temporary file   
        with tempfile.NamedTemporaryFile(delete=False) as temp_file:
            temp_file_path = temp_file.name
            print(f"Downloading file to {temp_file_path}")
            blob.download_to_filename(temp_file_path)
        

        # Check if the file exists after downloading
        if not os.path.exists(temp_file_path):
            raise FileNotFoundError(f"File not found: {temp_file_path}")
        
        # Open the downloaded file using the wave module
        with wave.open(temp_file_path, 'rb') as wav_file:
            num_channels = wav_file.getnchannels()
            sample_rate = wav_file.getframerate()
            print(f"Audio info - Channels: {num_channels}, Sample Rate: {sample_rate}")
            return {"num_channels": num_channels, "sample_rate": sample_rate}
    except Exception as e:
        print(f"Error getting audio info: {e}")
        return None
    finally:
        if temp_file_path and os.path.exists(temp_file_path):
            os.remove(temp_file_path)

def combine_results(response, timeout: int = 300, separator: str = " ") -> Optional[str]:
    try:
        #response = operation.result(timeout=timeout)
        results = []
        average_confidence = 0.0
        for result in response.results:
            if len(result.alternatives) > 1:
                most_likely_transcript = max(result.alternatives, key=lambda a: a.confidence)
                results.append(most_likely_transcript.transcript)
                average_confidence += most_likely_transcript.confidence
            else:
                results.append(result.alternatives[0].transcript)
                average_confidence += result.alternatives[0].confidence
        combined_text = separator.join(results)

        # Calculate average confidence (if there are transcripts)
        if results:
            average_confidence /= len(results)
            print(f"Average Confidence: {average_confidence}")
        return combined_text
    except Exception as e:
        print(f"Error processing results: {e}")
        return None
    
@app.get("/transcribe/")

async def transcribe_audio_file(gcs_uri: str):
    audio_info = get_audio_info(gcs_uri)
    if not audio_info:
        raise HTTPException(status_code=400, detail="Could not retrieve audio info from GCS URI")

    client_file = 'speechtotextapp-420710-009181d3c7f6.json'
    credentials = service_account.Credentials.from_service_account_file(client_file)
    client = speech.SpeechClient(credentials=credentials)

    # audio_info = get_audio_info(gcs_uri)
    # if not audio_info:
    #     return {"error": "Error getting audio information"}
        
    config = speech.RecognitionConfig(
        encoding=speech.RecognitionConfig.AudioEncoding.LINEAR16,
        #sample_rate_hertz=32000,
        sample_rate_hertz=audio_info["sample_rate"],
        language_code='en-US',
        #audio_channel_count=1
        audio_channel_count=audio_info["num_channels"]
    )
    
    audio = speech.RecognitionAudio(uri=gcs_uri)
    operation = client.long_running_recognize(config=config, audio=audio)

    print("Waiting for operation to complete...")
    response = operation.result(timeout=3000)
    
    combined_text = combine_results(response)  # Use the combine_results function

    if combined_text:
        return {"combinedTranscript": combined_text}
    else:
        return {"error": "Error processing results"}


    # transcriptions = []
    # for result in response.results:
    #     transcriptions.append(result.alternatives[0].transcript)

    # return {"transcriptions": transcriptions}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

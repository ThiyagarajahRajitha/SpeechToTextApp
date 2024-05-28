import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-transcription',
  templateUrl: './transcription.component.html',
  styleUrl: './transcription.component.css'
})
export class TranscriptionComponent {
  selectedFile: File | null = null;
  gsUtilUri$: Observable<string> | null = null; // Now an observable
  public gsUtilUri: string = '';
  public transcription: string ='';
  error: string = '';
  isTranscribing = false;

  constructor(private http: HttpClient) { }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    this.selectedFile = target.files?.[0] ?? null;
    this.gsUtilUri$ = null; // Reset transcription observable on new file selection
    this.error = '';
  }

  async transcribeFile() {
    if (!this.selectedFile) {
      this.error = 'No File found. Please upload a file to transcribe.';
      return;
    }

    //const formData = new FormData();
    //formData.append('file', this.selectedFile);
    let fileAppended = false;
    const formData = new FormData();
    if (!fileAppended) {
      formData.append('file', this.selectedFile);
      fileAppended = true; // Set flag after appending
      this.transcription = '';
      this.error = '';
    }
    // const formData = new FormData();
    try {
      this.isTranscribing = true; 
      this.error = '';
      this.transcription ='';
      this.gsUtilUri$ = this.http.post<any>('https://localhost:7081/api/speech/UploadFile', formData);
      this.gsUtilUri$.subscribe(
        (response: any) => {
          if (typeof response === 'object' && response.gsUtilUri) {
            //this.transcription = response.transcription;
            // Trigger the GetTranscription call automatically
            this.getTranscription(response.gsUtilUri);
            //this.isTranscribing = false; 
          } else {
            this.error = 'Unexpected error happened during transcription. Please check the file format or try again later.';
            console.error('Error during transcription:', response);
            this.isTranscribing = false; 
          }
        },
        error => {
          this.error = 'Unexpected error during uploading the file. Please check the file format or try again later.'
          console.error('Error during uploading:', error);
          this.isTranscribing = false; 
          // Handle other errors (optional)
        }
      );
    }
    catch (error) {
      this.error = 'Please try again.'
      console.error('Error during transcription:', error);
      this.isTranscribing = false; 
      // return null;
    }
  }
  getTranscription(gsUtilUri: string): void {
    this.isTranscribing = true; 
    // Assuming you want to call GetTranscription without any parameters
    this.http.get<any>('https://localhost:7081/api/speech/', { params: { gsUtilUri: gsUtilUri } })
      // .subscribe(
      //   (response: string) => {
      //     // Process the response as needed
      //     console.log('Transcription received:', response);
      //     this.transcription = response;
      //   },
      //   error => {
      //     console.error('Error getting transcription:', error);
      //     // Handle error (e.g., display an error message)
      //   }
      // );
      .subscribe(
        (response: any) => {
          if (typeof response === 'object' && response.transcription) {
            this.transcription = response.transcription;
            this.isTranscribing = false; 
            // Trigger the GetTranscription call automatically
            //this.getTranscription(response.transcription);
          } else {
            this.error = 'Unexpected error during transcription. Please check the file format or try again later.'
            console.error('Unexpected response format:', response);
            // Handle unexpected response format (e.g., display an error message)
            this.isTranscribing = false; 
          }
        }
      );
  }
  // sendEmail() {
  //   // Implement logic to send email using the transcription
  //   // You can access the transcription value using async/await with the observable
  //   this.transcription$ = this.transcription$ ?? new Observable<string>(); // Provide an empty observable as default
  //   this.transcription$.subscribe(async (transcription) => {
  //     console.log('Sending email with transcription:', transcription);
  //     // Send email logic here (using a library or service)
  //   });
  // }
}

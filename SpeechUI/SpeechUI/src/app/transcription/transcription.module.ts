import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { TranscriptionComponent } from './transcription.component';



@NgModule({
  declarations: [
    TranscriptionComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule
  ],
  exports: [
    TranscriptionComponent
    // Export the component if used elsewhere
  ]
})
export class TranscriptionModule { }

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TranscriptionModule } from './transcription/transcription.module';

@NgModule({
  declarations: [
    AppComponent 
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    TranscriptionModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

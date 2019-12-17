import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SplashComponent } from './components/splash/splash.component';

@NgModule({
  declarations: [SplashComponent],
  imports: [
    CommonModule
  ],
  exports: [
    CommonModule,
    FormsModule,
    SplashComponent
  ]
})
export class SharedModule { }

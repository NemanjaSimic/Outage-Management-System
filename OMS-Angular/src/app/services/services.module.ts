import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { SharedModule } from '@shared/shared.module';

import { EnvironmentService } from './environment/environment.service';

@NgModule({
  declarations: [],
  providers: [
    EnvironmentService
  ],
  imports: [
    HttpClientModule,
    SharedModule
  ]
})
export class ServicesModule { }

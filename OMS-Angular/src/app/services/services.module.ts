import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { SharedModule } from '@shared/shared.module';

import { EnvironmentService } from './environment/environment.service';
import { GraphService } from './notification/graph.service';
import { CommandService } from './command/command.service';

@NgModule({
  declarations: [],
  providers: [
    EnvironmentService,
    GraphService,
    CommandService
  ],
  imports: [
    HttpClientModule,
    SharedModule
  ]
})
export class ServicesModule { }

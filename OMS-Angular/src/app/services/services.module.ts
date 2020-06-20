import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { SharedModule } from '@shared/shared.module';

import { EnvironmentService } from './environment/environment.service';
import { GraphService } from './notification/graph.service';
import { CommandService } from './command/command.service';
import { OutageService } from './outage/outage.service';
import { ReportService } from './report/report.service';
import { DateFormatService } from './report/date-format.service';

@NgModule({
  declarations: [],
  providers: [
    EnvironmentService,
    GraphService,
    CommandService,
    OutageService,
    ReportService,
    DateFormatService
  ],
  imports: [
    HttpClientModule,
    SharedModule
  ]
})
export class ServicesModule { }

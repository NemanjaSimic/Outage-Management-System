import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotFoundComponent } from './not-found/not-found.component';

import { rootRoutes } from './routes.declaration';
import { CommonModule } from '@angular/common';
import { ReportModule } from '@modules/report/report.module';
import { GraphModule } from '@modules/graph/graph.module';

@NgModule({
  declarations: [
    NotFoundComponent
  ],
  imports: [
    CommonModule,
    ReportModule,
    GraphModule,
    RouterModule.forRoot(rootRoutes)
  ],
  exports:[
    RouterModule
  ]
})
export class RoutingModule { }

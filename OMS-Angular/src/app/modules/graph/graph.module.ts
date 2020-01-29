import { NgModule } from '@angular/core';
import { GraphComponent } from './components/graph/graph.component';
import { ServicesModule } from '@services/services.module';
import { SharedModule } from '@shared/shared.module';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [
    GraphComponent
  ],
  imports: [
    SharedModule,
    ServicesModule,
    RouterModule.forChild([])
  ]
})
export class GraphModule { }
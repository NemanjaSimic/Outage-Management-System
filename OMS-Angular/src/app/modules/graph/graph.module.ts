import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GraphComponent } from './components/graph/graph.component';
import { ServicesModule } from '@services/services.module';


@NgModule({
  declarations: [GraphComponent],
  imports: [
    CommonModule,
    ServicesModule
  ]
})
export class GraphModule { }
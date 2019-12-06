import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GraphComponent } from './components/graph/graph.component';

import { CytoscapeModule } from 'ngx-cytoscape';

@NgModule({
  declarations: [GraphComponent],
  imports: [
    CommonModule,
    CytoscapeModule
  ]
})
export class GraphModule { }
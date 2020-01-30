import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotFoundComponent } from './not-found/not-found.component';

import { rootRoutes } from './routes.declaration';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    NotFoundComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forRoot(rootRoutes)
  ],
  exports:[
    RouterModule
  ]
})
export class RoutingModule { }

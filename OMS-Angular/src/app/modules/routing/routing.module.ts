import { NgModule } from '@angular/core';
import { RouterModule, Route } from '@angular/router';
import { NotFoundComponent } from './not-found/not-found.component';

import { rootRoutes } from './routes.declaration';

@NgModule({
  declarations: [
    NotFoundComponent
  ],
  imports: [
    RouterModule.forRoot(rootRoutes)
  ],
  exports:[
    RouterModule
  ]
})
export class RoutingModule { }

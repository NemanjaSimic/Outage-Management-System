import { Route } from '@angular/router';
import { AppComponent } from 'app/app.component';
import { NotFoundComponent } from './not-found/not-found.component';

const rootRoutes: Route[] = [
  { path: '', component: AppComponent },
  { path: '**', component: NotFoundComponent }
]

export default rootRoutes;
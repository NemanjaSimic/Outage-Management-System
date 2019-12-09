import { Route } from '@angular/router';
import { AppComponent } from 'app/app.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { GraphComponent } from '@modules/graph/components/graph/graph.component';

export const rootRoutes: Route[] = [
  { path: '', component: AppComponent, pathMatch: 'full' },
  { path: 'graph', component: GraphComponent },
  { path: '**', component: NotFoundComponent }
]

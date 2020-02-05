import { Route } from '@angular/router';
import { NotFoundComponent } from './not-found/not-found.component';
import { GraphComponent } from '@modules/graph/components/graph/graph.component';
import { ActiveBrowserComponent } from '@modules/active-browser/active-browser.component';

export const rootRoutes: Route[] = [
  { path: '', component: GraphComponent },
  { path: 'graph', component: GraphComponent },
  { path: 'active-browser', component: ActiveBrowserComponent },
  { path: '**', component: NotFoundComponent }
]

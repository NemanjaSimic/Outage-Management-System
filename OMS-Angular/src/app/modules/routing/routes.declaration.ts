import { Route } from '@angular/router';
// import { AppComponent } from 'app/app.component';
import { NotFoundComponent } from './not-found/not-found.component';

export const rootRoutes: Route[] = [
  { 
    path: '', 
    loadChildren: () => import('../graph/graph.module').then(m => m.GraphModule) 
  },
  { 
    path: 'graph', 
    loadChildren: () => import('../graph/graph.module').then(m => m.GraphModule) 
  },
  { path: '**', component: NotFoundComponent }
]

import { Route } from '@angular/router';
// import { AppComponent } from 'app/app.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { GraphComponent } from '@modules/graph/components/graph/graph.component';
import { SplashComponent } from '@shared/components/splash/splash.component';
import { ActiveBrowserComponent } from '@modules/active-browser/active-browser.component';
import { HistoricalBrowserComponent } from '@modules/historical-browser/historical-browser.component';

export const rootRoutes: Route[] = [
  { path: '', component: GraphComponent },
  { path: 'graph', component: GraphComponent },
  { path: 'splash', component: SplashComponent },
  { path: 'active_browser', component: ActiveBrowserComponent },
  { path: 'historical_browser', component: HistoricalBrowserComponent },
  { path: '**', component: NotFoundComponent }
]

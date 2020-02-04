import { Route } from '@angular/router';
// import { AppComponent } from 'app/app.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { GraphComponent } from '@modules/graph/components/graph/graph.component';
import { SplashComponent } from '@shared/components/splash/splash.component';
import { ActiveBrowserComponent } from '@modules/active-browser/active-browser.component';
// import { HistoricalBrowserComponent } from '@modules/historical-browser/historical-browser.component';

export const rootRoutes: Route[] = [
  { path: '', component: GraphComponent },
  { path: 'graph', component: GraphComponent },
  { path: 'splash', component: SplashComponent },
<<<<<<< 3350cb88550753de3fb17c39460a84c4edcaeef5
  { path: 'active-browser', component: ActiveBrowserComponent },
=======
  { path: 'active_browser', component: ActiveBrowserComponent },
>>>>>>> WEB: outage query handling
  // { path: 'historical_browser', component: HistoricalBrowserComponent },
  { path: '**', component: NotFoundComponent }
]

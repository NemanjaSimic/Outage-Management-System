import { NgModule } from '@angular/core';

import { RoutingModule } from '@modules/routing/routing.module';
import { SharedModule } from '@shared/shared.module';
import { ServicesModule } from '@services/services.module';
import { MatSidenavModule } from '@angular/material/sidenav';
import { GraphModule } from './modules/graph/graph.module';

import { AppComponent } from './app.component';
import { ActiveBrowserComponent } from './modules/active-browser/active-browser.component';
import { ArchivedBrowserComponent } from './modules/archived-browser/archived-browser.component';

@NgModule({
  declarations: [
    AppComponent,
    ActiveBrowserComponent,
    ArchivedBrowserComponent
  ],
  imports: [
    RoutingModule,
    SharedModule,
    ServicesModule,
    GraphModule,
    MatSidenavModule    
  ],
  providers: [
    ServicesModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

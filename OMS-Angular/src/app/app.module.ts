import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { RoutingModule } from '@modules/routing/routing.module';
import { SharedModule } from '@shared/shared.module';
import { ServicesModule } from '@services/services.module';

import { GraphModule } from './modules/graph/graph.module';
import { ActiveBrowserComponent } from './modules/active-browser/active-browser.component';
import { HistoricalBrowserComponent } from './modules/historical-browser/historical-browser.component';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

@NgModule({
  declarations: [
    AppComponent,
    ActiveBrowserComponent,
    HistoricalBrowserComponent
  ],
  imports: [
    BrowserModule,
    RoutingModule,
    SharedModule,
    ServicesModule,
    GraphModule,
    MatSidenavModule,
    MatCheckboxModule,
    MatListModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule
  ],
  providers: [
    ServicesModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

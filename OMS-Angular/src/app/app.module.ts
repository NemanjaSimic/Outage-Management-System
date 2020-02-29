import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { RoutingModule } from '@modules/routing/routing.module';
import { SharedModule } from '@shared/shared.module';
import { ServicesModule } from '@services/services.module';
import { GraphModule } from './modules/graph/graph.module';
import { ActiveBrowserComponent } from './modules/active-browser/active-browser.component';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { ArchivedBrowserComponent } from './modules/archived-browser/archived-browser.component';
import { ModalComponent } from './modules/modal/modal.component';
import { MatDialogModule} from '@angular/material';

@NgModule({
  declarations: [
    AppComponent,
    ActiveBrowserComponent,
    ArchivedBrowserComponent,
    ModalComponent
  ],
  imports: [
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
    MatTableModule,
    MatDialogModule
  ],
  providers: [
    ServicesModule
  ],
  bootstrap: [AppComponent],
  entryComponents: [ModalComponent]
})
export class AppModule { }

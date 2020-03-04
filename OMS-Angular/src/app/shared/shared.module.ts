import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';

import { SplashComponent } from './components/splash/splash.component';
import { ActiveOutageModalComponent } from './components/active-outage-modal/active-outage-modal.component';
import { ArchivedOutageModalComponent } from './components/archived-outage-modal/archived-outage-modal.component';


@NgModule({
  declarations: [
    SplashComponent,
    ActiveOutageModalComponent,
    ArchivedOutageModalComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    MatSidenavModule,
    MatCheckboxModule,
    MatListModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatDialogModule
  ],
  exports: [
    CommonModule,
    FormsModule,
    SplashComponent,
    MatSidenavModule,
    MatCheckboxModule,
    MatListModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatDialogModule
  ],
  entryComponents: [
    ActiveOutageModalComponent,
    ArchivedOutageModalComponent
  ]
})
export class SharedModule { }

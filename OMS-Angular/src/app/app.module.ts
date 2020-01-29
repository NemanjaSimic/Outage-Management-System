import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { RoutingModule } from '@modules/routing/routing.module';
import { SharedModule } from '@shared/shared.module';
import { ServicesModule } from '@services/services.module';
import { GraphModule } from '@modules/graph/graph.module';


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    RoutingModule,
    SharedModule,
    ServicesModule,
    GraphModule
  ],
  providers: [
    ServicesModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

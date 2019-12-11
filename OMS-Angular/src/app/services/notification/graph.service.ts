import { Injectable, EventEmitter } from '@angular/core';
import { EnvironmentService } from '@services/environment/environment.service';
import { OmsGraph } from '@shared/models/oms-graph.model';
import { Observable, Observer } from 'rxjs';

// TODO: add jquery in a different way, this may result in prod. build errors
declare var $;

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  public updateRecieved: EventEmitter<OmsGraph>;

  private proxy: any;
  private connection: any;
  private proxyName: string = 'graphhub';

  constructor(private envService: EnvironmentService) {
    this.updateRecieved = new EventEmitter<OmsGraph>();

    this.connection = $.hubConnection(`${this.envService.serverUrl}`);
    this.proxy = this.connection.createHubProxy(this.proxyName);

    this.registerGraphUpdateListener();
  }

  public startConnection(): Observable<boolean> {
    return Observable.create((observer: Observer<boolean>) => {

      this.connection.start(() => {
        this.proxy.invoke("join");
      })
        .done(() => {
          observer.next(true);
          observer.complete();
        })
        .fail((error: any) => {
          console.log('Could not connect ' + error);

          observer.next(false);
          observer.complete();
        });
    });
  }

  public registerGraphUpdateListener(): void {
    this.proxy.on('updateGraph', (data: OmsGraph) => {
      this.updateRecieved.emit(data);
    });
  }

}

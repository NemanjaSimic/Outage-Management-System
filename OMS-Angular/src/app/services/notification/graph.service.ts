import { Injectable, EventEmitter } from '@angular/core';
import { EnvironmentService } from '@services/environment/environment.service';
import { OmsGraph } from '@shared/models/oms-graph.model';
import { Observable, Observer } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ScadaData } from '@shared/models/scada-data.model';

// TODO: add jquery in a different way, this may result in prod. build errors
declare var $;

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  public updateRecieved: EventEmitter<OmsGraph>;
  public outageRecieved: EventEmitter<Number>;

  private proxy: any;
  private connection: any;
  private proxyName: string = 'graphhub';

  constructor(private envService: EnvironmentService, private http: HttpClient) {
    this.updateRecieved = new EventEmitter<OmsGraph>();
    this.outageRecieved = new EventEmitter<Number>();

    this.connection = $.hubConnection(`${this.envService.serverUrl}`);
    this.proxy = this.connection.createHubProxy(this.proxyName);

    this.registerGraphUpdateListener();
    this.registerOutageListener();
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

  public registerOutageListener(): void {
    this.proxy.on('reportOutageCall', (gid: Number) => {
      this.outageRecieved.emit(gid);
    });
  }

  public getTopology(): Observable<OmsGraph> {
    // /test je endpoint gde se dobiju mock podaci sa spojenim transformatorima
    return this.http.get(`${this.envService.apiUrl}/test`) as Observable<OmsGraph>;
    // return this.http.get(`${this.envService.apiUrl}/topology`) as Observable<OmsGraph>;
  }

}

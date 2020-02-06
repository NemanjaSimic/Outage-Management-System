import { Injectable, EventEmitter } from '@angular/core';
import { EnvironmentService } from '@services/environment/environment.service';
import { Observable, Observer } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ScadaData } from '@shared/models/scada-data.model';
import { ArchivedOutage, ActiveOutage } from '@shared/models/outage.model';

// TODO: add jquery in a different way, this may result in prod. build errors
declare var $;

@Injectable({
  providedIn: 'root'
})
export class OutageService {
  public activeOutageUpdateRecieved: EventEmitter<ActiveOutage>;
  public archivedOutageUpdateRecieved: EventEmitter<ArchivedOutage>;

  private proxy: any;
  private connection: any;
  private proxyName: string = 'outagehub';

  constructor(private envService: EnvironmentService, private http: HttpClient) {
    this.activeOutageUpdateRecieved = new EventEmitter<ActiveOutage>();
    this.archivedOutageUpdateRecieved = new EventEmitter<ArchivedOutage>();

    this.connection = $.hubConnection(`${this.envService.serverUrl}`);
    this.proxy = this.connection.createHubProxy(this.proxyName);

    this.registerActiveOutageUpdateListener();
    this.registerArchivedOutageUpdateListener();
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

  public registerActiveOutageUpdateListener(): void {
    this.proxy.on('activeOutageUpdate', (data: ActiveOutage) => {
      this.activeOutageUpdateRecieved.emit(data);
    });
  }

  public registerArchivedOutageUpdateListener(): void {
    this.proxy.on('archivedOutageUpdate', (data: ArchivedOutage) => {
      this.archivedOutageUpdateRecieved.emit(data);
    });
  }
}

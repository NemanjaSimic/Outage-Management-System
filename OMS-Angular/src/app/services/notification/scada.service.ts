import { Injectable, EventEmitter } from '@angular/core';
import { EnvironmentService } from '@services/environment/environment.service';
import { Observable, Observer } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ScadaData } from '@shared/models/scada-data.model';

// TODO: add jquery in a different way, this may result in prod. build errors
declare var $;

@Injectable({
  providedIn: 'root'
})
export class ScadaService {
  public updateRecieved: EventEmitter<ScadaData>;

  private proxy: any;
  private connection: any;
  private proxyName: string = 'scadahub';

  constructor(private envService: EnvironmentService, private http: HttpClient) {
    this.updateRecieved = new EventEmitter<ScadaData>();

    this.connection = $.hubConnection(`${this.envService.serverUrl}`);
    this.proxy = this.connection.createHubProxy(this.proxyName);

    this.registerScadaDataUpdateListener();
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

  public registerScadaDataUpdateListener(): void {
    this.proxy.on('updateScadaData', (data: ScadaData) => {
      this.updateRecieved.emit(data);
    });
  }

//   public getTopology(): Observable<ScadaData> {
//     return this.http.get(`${this.envService.apiUrl}/topology`) as Observable<ScadaData>;
//   }

}

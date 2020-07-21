import { Injectable, EventEmitter } from '@angular/core';
import * as signalR from "@aspnet/signalr";
import { OmsGraph } from '@shared/models/oms-graph.model';
import { environment } from '@env/environment';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
 
@Injectable({
  providedIn: 'root'
})
export class GraphCoreService {
    public updateRecieved: EventEmitter<OmsGraph>;
    public outageRecieved: EventEmitter<Number>;

    private hubConnection: signalR.HubConnection
    private hubName: string = 'graphhub';
 
  constructor(private http: HttpClient) {
    this.updateRecieved = new EventEmitter<OmsGraph>();
    this.outageRecieved = new EventEmitter<Number>();
  }

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl(`${environment.serverUrl}/${this.hubName}`)
                            .build();
 
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))

    this.registerGraphUpdateListener();
    this.registerOutageListener();
  }
 
  public registerGraphUpdateListener = () => {
    this.hubConnection.on('updateGraph', (data: OmsGraph) => {
      this.updateRecieved.emit(data);
    });
  }

  public registerOutageListener = () => {
    this.hubConnection.on('reportOutageCall', (gid: Number) => {
      this.outageRecieved.emit(gid);
    });
  }

  public getTopology(): Observable<OmsGraph> {
    return this.http.get(`${environment.apiUrl}/topology`) as Observable<OmsGraph>;
  }
}
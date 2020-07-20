import { Injectable, EventEmitter } from '@angular/core';
import * as signalR from "@aspnet/signalr";
import { OmsGraph } from '@shared/models/oms-graph.model';
import { environment } from '@env/environment';
 
@Injectable({
  providedIn: 'root'
})
export class GraphCoreService {
    // public updateRecieved: EventEmitter<OmsGraph>;
    // public outageRecieved: EventEmitter<Number>;
    private hubConnection: signalR.HubConnection
    private hubName: string = 'graphhub';
 
  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl(`${environment.serverUrl}\\${this.hubName}`)
                            .build();
 
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))
  }
 
  public registerOutageListener = () => {
    this.hubConnection.on('reportOutageCall', (gid: Number) => {
        // this.outageRecieved.emit(gid);
    });
  }
}
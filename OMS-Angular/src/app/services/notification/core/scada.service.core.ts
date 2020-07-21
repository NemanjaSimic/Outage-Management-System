import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { environment } from '@env/environment';

@Injectable({
    providedIn: 'root'
  })
export class ScadaCoreService {
    private hubConnection: signalR.HubConnection
    private hubName: string = 'scadahub';
 
    public startConnection = () => {
      this.hubConnection = new signalR.HubConnectionBuilder()
                              .withUrl(`${environment.serverUrl}/${this.hubName}`)
                              .build();
   
      this.hubConnection
        .start()
        .then(() => console.log('Connection started'))
        .catch(err => console.log('Error while starting connection: ' + err))
    }
}
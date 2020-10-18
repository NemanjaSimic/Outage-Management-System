import { Injectable, EventEmitter } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { environment } from '@env/environment';
import { ActiveOutage, ArchivedOutage } from '@shared/models/outage.model';

@Injectable({
    providedIn: 'root'
  })
export class OutageNotificationCoreService {
    public activeOutageUpdateRecieved: EventEmitter<ActiveOutage>;
    public archivedOutageUpdateRecieved: EventEmitter<ArchivedOutage>;

    private hubConnection: signalR.HubConnection
    private hubName: string = 'outagehub';
 
    constructor() {
      this.activeOutageUpdateRecieved = new EventEmitter<ActiveOutage>();
      this.archivedOutageUpdateRecieved = new EventEmitter<ArchivedOutage>();
    }

    public startConnection = () => {
      this.hubConnection = new signalR.HubConnectionBuilder()
                              .withUrl(`${environment.serverUrl}/${this.hubName}`)
                              .build();
                              
     this.hubConnection.serverTimeoutInMilliseconds = 1000 * 1800;
      
      this.hubConnection.onclose((err) => { 
        console.log('Disconnected from outage: ' + err)
        this.startConnection();
      })    
   
      this.hubConnection
        .start()
        .then(() => {
          console.log('Connected to Outage Notification service');
          this.registerActiveOutageUpdateListener();
          this.registerArchivedOutageUpdateListener();
        })
        .catch(err => console.log('Could not connect to Outage Notification service: ' + err))
    }

    public registerActiveOutageUpdateListener = () => {
    this.hubConnection.on('activeOutageUpdate', (jsonData: string) => {
      let activeOutageData : ActiveOutage = JSON.parse(jsonData); 
      this.activeOutageUpdateRecieved.emit(activeOutageData);
    });
  }
  
    public registerArchivedOutageUpdateListener = () => {
      this.hubConnection.on('archivedOutageUpdate', (jsonData: string) => {
        let archivedOutageData : ArchivedOutage = JSON.parse(jsonData); 
        this.archivedOutageUpdateRecieved.emit(archivedOutageData);
      });
    }
}
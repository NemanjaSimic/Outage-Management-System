import { Injectable, EventEmitter } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { environment } from '@env/environment';
import { ScadaData } from '@shared/models/scada-data.model';

@Injectable({
    providedIn: 'root'
  })
export class ScadaCoreService {
    public updateRecieved: EventEmitter<ScadaData>;

    private hubConnection: signalR.HubConnection
    private hubName: string = 'scadahub';
 
    constructor() {
        this.updateRecieved = new EventEmitter<ScadaData>();
    }
    
    public startConnection = () => {
        this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${environment.serverUrl}/${this.hubName}`)
        .build();
        
        this.hubConnection
        .start()
        .then(() => {
            console.log('Connected to scada service');
            this.registerScadaDataUpdateListener();
        })
        .catch(err => console.log('Could not connect to scada service: ' + err))
    }

    public registerScadaDataUpdateListener = () => {
        this.hubConnection.on('updateScadaData', (data: ScadaData) => {
            this.updateRecieved.emit(data);
        });
    }
}
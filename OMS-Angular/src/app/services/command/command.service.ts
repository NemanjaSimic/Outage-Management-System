import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { SwitchCommand } from '@shared/models/switch-command.model';
import { EnvironmentService } from '@services/environment/environment.service';

@Injectable({
  providedIn: 'root'
})
export class CommandService {

  constructor(
    private http: HttpClient, 
    private envService: EnvironmentService
  ) { }

  sendSwitchCommand(command: SwitchCommand): Observable<any> {
    console.log(this.http);
    return this.http.post(`${this.envService.apiUrl}/scada`, command);
  }

}

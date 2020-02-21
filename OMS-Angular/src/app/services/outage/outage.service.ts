import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { EnvironmentService } from '@services/environment/environment.service';
import { ActiveOutage, ArchivedOutage } from '@shared/models/outage.model';

@Injectable({
  providedIn: 'root'
})
export class OutageService {

  constructor(
    private http: HttpClient, 
    private envService: EnvironmentService
  ) { }

  getAllActiveOutages(): Observable<ActiveOutage[]> {
    return this.http.get(`${this.envService.apiUrl}/outage/active`) as Observable<ActiveOutage[]>;
  }

  getAllArchivedOutages(): Observable<ArchivedOutage[]> {
    return this.http.get(`${this.envService.apiUrl}/outage/archived`) as Observable<ArchivedOutage[]>;
  }

  isolateOutageCommand(id: Number) : Observable<any>{
    console.log(this.http);
    return this.http.post(`${this.envService.apiUrl}/outage/isolate/${id}`);
  }

  sendCrewOutageCommand(id: Number) : Observable<any>{
    console.log(this.http);
    return this.http.post(`${this.envService.apiUrl}/outage/sendcrew/${id}`);
  }

  resolveOutageCommand(id: Number) : Observable<any>{
    console.log(this.http);
    return this.http.post(`${this.envService.apiUrl}/outage/resolve/${id}`);
  }

  validateOutageCommand(id: Number) : Observable<any>{
    console.log(this.http);
    return this.http.post(`${this.envService.apiUrl}/outage/validate/${id}`);
  }

}

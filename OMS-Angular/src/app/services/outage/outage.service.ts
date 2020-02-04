import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { EnvironmentService } from '@services/environment/environment.service';
import { ActiveOutage } from '@shared/models/outage.model';

@Injectable({
  providedIn: 'root'
})
export class OutageService {

  constructor(
    private http: HttpClient, 
    private envService: EnvironmentService
  ) { }

  getAllActiveOutages(): Observable<any> {
    console.log(this.http);
    return this.http.get(`${this.envService.apiUrl}/outage/getActive`);
  }

  getAllArchivedOutages(): Observable<any> {
    console.log(this.http);
    return this.http.get(`${this.envService.apiUrl}/outage/getArchived`);
  }
}

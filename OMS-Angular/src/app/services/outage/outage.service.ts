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
}

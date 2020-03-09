import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvironmentService } from '@services/environment/environment.service';
import { ReportOptions } from '@shared/models/report-options.model';
import { Observable } from 'rxjs';
import { Report } from '@shared/models/report.model';
import { buildQuery } from './query-builder';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  constructor(
    private http: HttpClient, 
    private envService: EnvironmentService
  ) { }

  public generateReport(options: ReportOptions) : Observable<Report> {
    const query = buildQuery(options);
    if(!query) return;
    
    return this.http.get(`${this.envService.apiUrl}/report?${query}`) as Observable<Report>;
  }

  
}

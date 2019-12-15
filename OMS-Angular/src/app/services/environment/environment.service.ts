import { Injectable } from '@angular/core';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {
  public serverUrl: string = environment.serverUrl;
  public apiUrl: string = environment.apiUrl; 
}

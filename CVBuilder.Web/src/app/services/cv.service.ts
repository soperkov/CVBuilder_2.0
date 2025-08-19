import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CVService {
  private apiUrl = 'https://localhost:7123/api/cv';

  constructor(private http: HttpClient) {}

  createCv(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }
}

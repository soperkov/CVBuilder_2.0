import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CVService {
  private apiUrl = 'https://localhost:7123/api/cv';

  constructor(private http: HttpClient) {}

  // Create CV
  createCV(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  // Get all CVs for the logged-in user
  getMyCVs(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // Get a specific CV by ID
  getCVById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  // Update existing CV by ID
  updateCV(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  // Delete CV by ID
  deleteCV(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // Multiple delete CV
  deleteMany(ids: number[]): Observable<void> {
    const params = new HttpParams().set('ids', ids.join(','));
    return this.http.delete<void>(`${this.apiUrl}`, { params });
  }
}

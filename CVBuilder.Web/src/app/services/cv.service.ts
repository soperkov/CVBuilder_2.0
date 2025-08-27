import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Cv, CreateCvDto, UpdateCvDto } from '../models';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CVService {
  private base = `${environment.apiBaseUrl}/cv`;

  constructor(private http: HttpClient) {}

  // Create CV
  createCV(data: CreateCvDto): Observable<number> {
    return this.http.post<number>(this.base, data);
  }

  // Get all CVs for the logged-in user
  getMyCVs(): Observable<any[]> {
    return this.http.get<any[]>(this.base).pipe(
      map((items) =>
        (items || []).map((x) => ({
          ...x,
          createdAt: x.createdAt ?? x.createdAtUtc ?? null,
          modifiedAt: x.modifiedAt ?? x.updatedAtUtc ?? null,
        }))
      )
    );
  }

  // Get a specific CV by ID
  getCVById(id: number): Observable<any> {
    return this.http.get<any>(`${this.base}/${id}`).pipe(
      map((x) => ({
        ...x,
        createdAt: x.createdAt ?? x.createdAtUtc ?? null,
        modifiedAt: x.modifiedAt ?? x.updatedAtUtc ?? null,
      }))
    );
  }

  // Update existing CV by ID
  updateCV(id: number, data: UpdateCvDto): Observable<any> {
    return this.http.put(`${this.base}/${id}`, data);
  }

  // Delete CV by ID
  deleteCV(id: number): Observable<any> {
    return this.http.delete(`${this.base}/${id}`);
  }

  // Multiple delete CV
  deleteMany(ids: number[]): Observable<void> {
    const params = new HttpParams().set('ids', ids.join(','));
    return this.http.delete<void>(`${this.base}`, { params });
  }

  getPdfTicket(id: number) {
    return this.http.post<{ token: string }>(
      `${environment.apiBaseUrl}/cv/${id}/pdf-ticket`,
      {}
    );
  }

  getCvPhotoBlob(id: number) {
    return this.http.get(`${this.base}/${id}/photo`, { responseType: 'blob' });
  }
}

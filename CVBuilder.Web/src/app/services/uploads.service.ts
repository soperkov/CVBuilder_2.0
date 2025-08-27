import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface UploadResponse {
  path: string;
  fileName: string;
  size: number;
  contentType: string;
}

@Injectable({ providedIn: 'root' })
export class UploadsService {
  private base = `${environment.apiBaseUrl}/uploads`;

  constructor(private http: HttpClient) {}

  uploadPhoto(file: File): Observable<UploadResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<UploadResponse>(`${this.base}/photo`, form);
  }

  getPhotoAsBlob(path: string): Observable<Blob> {
    const params = new HttpParams().set('path', path);
    return this.http.get(`${this.base}/photo`, {
      params,
      responseType: 'blob',
    });
  }
}

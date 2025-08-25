import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface TemplateDto {
  id: number;
  name: string;
  description?: string | null;
  cssContent: string;
  previewImageUrl?: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private base = `${environment.apiBaseUrl}/template`;

  constructor(private http: HttpClient) {}

  getById(id: number): Observable<TemplateDto> {
    return this.http.get<TemplateDto>(`${this.base}/${id}`);
  }

  getAll(): Observable<TemplateDto[]> {
    return this.http.get<TemplateDto[]>(this.base);
  }
}

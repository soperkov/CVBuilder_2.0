import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private apiUrl = 'https://localhost:7123/api/template';

  constructor(private http: HttpClient) {}

  getById(id: number): Observable<TemplateDto> {
    return this.http.get<TemplateDto>(`${this.apiUrl}/${id}`);
  }

  getAll(): Observable<TemplateDto[]> {
    return this.http.get<TemplateDto[]>(this.apiUrl);
  }
}

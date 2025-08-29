// services/template.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface TemplateOption {
  id: number;
  name: string;
  thumbUrl: string; // absolute URL
}

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private base = `${environment.apiBaseUrl}/template`;
  constructor(private http: HttpClient) {}

  getAll(): Observable<TemplateOption[]> {
    return this.http
      .get<{ id: number; name: string; thumbUrl?: string }[]>(this.base)
      .pipe(
        map((list) =>
          (list ?? []).map((t) => ({
            id: t.id,
            name: t.name,
            thumbUrl:
              t.thumbUrl ?? `${environment.apiBaseUrl}/templates/${t.name}.png`,
          }))
        )
      );
  }

  getById(id: number): Observable<TemplateOption> {
    return this.http
      .get<{ id: number; name: string; thumbUrl?: string }>(
        `${this.base}/${id}`
      )
      .pipe(
        map((t) => ({
          id: t.id,
          name: t.name,
          thumbUrl:
            t.thumbUrl ?? `${environment.apiBaseUrl}/templates/${t.name}.png`,
        }))
      );
  }
}

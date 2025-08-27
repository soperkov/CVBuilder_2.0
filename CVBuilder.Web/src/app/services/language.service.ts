import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface LangOption {
  id: number;
  code: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly base = `${environment.apiBaseUrl}/language`;
  constructor(private http: HttpClient) {}
  getAll(): Observable<LangOption[]> {
    return this.http.get<LangOption[]>(this.base);
  }
  getById(id: number): Observable<LangOption> {
    return this.http.get<LangOption>(`${this.base}/${id}`);
  }
}

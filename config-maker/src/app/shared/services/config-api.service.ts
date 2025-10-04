import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BalanceVersion, BalancePayload } from '../models/balance-config.model';

interface ConfigVersionListResponse {
  items: BalanceVersion[];
  total: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ConfigApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/v1/config';

  getVersions(): Observable<BalanceVersion[]> {
    return this.http.get<ConfigVersionListResponse>(`${this.apiUrl}/versions`).pipe(
      map(response => response.items)
    );
  }

  getActiveVersion(): Observable<BalanceVersion> {
    return this.http.get<BalanceVersion>(`${this.apiUrl}/active`);
  }

  getPayload(versionId: string): Observable<string> {
    return this.http.get(`${this.apiUrl}/versions/${versionId}/payload`, { 
      responseType: 'text'
    });
  }

  savePayload(versionId: string, json: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/versions/${versionId}/payload`, { json });
  }

  publishVersion(versionId: string): Observable<BalanceVersion> {
    return this.http.post<BalanceVersion>(`${this.apiUrl}/versions/${versionId}/publish`, {});
  }

  createVersion(version: string, comment: string): Observable<BalanceVersion> {
    return this.http.post<BalanceVersion>(`${this.apiUrl}/versions`, {
      version,
      comment
    });
  }
}


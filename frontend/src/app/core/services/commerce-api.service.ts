import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ProcessCommerceResult,
  ProcessDateSummary,
  QuarantinedCommerce,
  UploadCommerceFileResult
} from '../models/commerce.models';

@Injectable({ providedIn: 'root' })
export class CommerceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/commerce`;

  upload(file: File): Observable<UploadCommerceFileResult> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.post<UploadCommerceFileResult>(`${this.baseUrl}/upload`, body);
  }

  process(processDate: string): Observable<ProcessCommerceResult> {
    return this.http.post<ProcessCommerceResult>(`${this.baseUrl}/process`, { processDate });
  }

  getQuarantine(processDate?: string | null): Observable<QuarantinedCommerce[]> {
    const params = processDate ? new HttpParams().set('processDate', processDate) : undefined;
    return this.http.get<QuarantinedCommerce[]>(`${this.baseUrl}/quarantine`, { params });
  }

  getProcessDates(): Observable<ProcessDateSummary[]> {
    return this.http.get<ProcessDateSummary[]>(`${this.baseUrl}/process-dates`);
  }
}

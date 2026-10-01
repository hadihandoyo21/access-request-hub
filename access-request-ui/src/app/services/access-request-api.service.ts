import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccessRequest, CreateRequestDto, ApproveRequestDto, RejectRequestDto } from '../models/access-request.model';

@Injectable({
  providedIn: 'root'
})
export class AccessRequestApiService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:715x/api/requests'; // Update with your actual API port

  getRequests(): Observable<AccessRequest[]> {
    return this.http.get<AccessRequest[]>(this.apiUrl);
  }

  createRequest(dto: CreateRequestDto): Observable<AccessRequest> {
    return this.http.post<AccessRequest>(this.apiUrl, dto);
  }

  approveRequest(id: number, dto: ApproveRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/approve`, dto);
  }

  rejectRequest(id: number, dto: RejectRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/reject`, dto);
  }
}
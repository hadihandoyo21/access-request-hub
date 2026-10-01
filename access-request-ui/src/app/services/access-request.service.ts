import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { UserService } from './user.service';

export interface AccessRequest {
  id: number;
  requester: string;
  application: string;
  environment: string;
  accessLevel: string;
  status: string;
  version: number;
}

@Injectable({
  providedIn: 'root'
})
export class AccessRequestService {
  private http = inject(HttpClient);
  private userService = inject(UserService);
  private baseUrl = 'http://localhost:5020/api/requests';

  // Helper to attach the X-Mock-User header based on current dropdown selection
  private getOptions() {
    return {
      headers: new HttpHeaders({
        'X-Mock-User': this.userService.currentUser()
      })
    };
  }

  getRequests() {
    return this.http.get<AccessRequest[]>(this.baseUrl, this.getOptions());
  }

  createRequest(dto: any) {
    return this.http.post(this.baseUrl, dto, this.getOptions());
  }

  approveRequest(id: number, version: number) {
    return this.http.post(`${this.baseUrl}/${id}/approve`, { version }, this.getOptions());
  }

  rejectRequest(id: number, version: number, reason: string) {
    return this.http.post(`${this.baseUrl}/${id}/reject`, { version, reason }, this.getOptions());
  }
}
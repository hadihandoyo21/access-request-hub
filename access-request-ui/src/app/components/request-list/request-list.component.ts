import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AccessRequestService, AccessRequest } from '../../services/access-request.service';

@Component({
  selector: 'app-request-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './request-list.component.html'
})
export class RequestListComponent implements OnInit {
  requests: AccessRequest[] = [];

  constructor(private requestService: AccessRequestService) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.requestService.getRequests().subscribe({
      next: (data) => this.requests = data,
      error: (err) => console.error('Error fetching requests', err)
    });
  }

  onApprove(req: AccessRequest): void {
    this.requestService.approveRequest(req.id, req.version).subscribe({
      next: () => this.loadRequests(),
      error: (err) => alert(err.error?.message || 'Error approving request')
    });
  }

  onReject(req: AccessRequest): void {
    const reason = prompt('Enter rejection reason:');
    if (!reason) return;

    this.requestService.rejectRequest(req.id, req.version, reason).subscribe({
      next: () => this.loadRequests(),
      error: (err) => alert(err.error?.message || 'Error rejecting request')
    });
  }
}
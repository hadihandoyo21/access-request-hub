import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from './services/user.service';
import { AccessRequestService, AccessRequest } from './services/access-request.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="app-container">
      <header class="header">
        <h1>Access Request Hub</h1>
        <div class="user-switcher">
          <label><strong>Active User:</strong></label>
          <select [ngModel]="currentActiveUser" (ngModelChange)="switchUser($any($event))">
            <option value="alice&#64;example.local">alice&#64;example.local (Requester)</option>
            <option value="bob&#64;example.local">bob&#64;example.local (Manager)</option>
            <option value="carol&#64;example.local">carol&#64;example.local (CRM Owner)</option>
            <option value="dana&#64;example.local">dana&#64;example.local (Finance Owner)</option>
            <option value="erin&#64;example.local">erin&#64;example.local (Admin/Auditor)</option>
          </select>
        </div>
      </header>

      <main class="content">
        @if (message()) {
          <div class="alert" [class.error]="isError()">{{ message() }}</div>
        }

        <section class="card">
          <h2>Create New Access Request</h2>
          <form (submit)="onSubmit($event)">
            <div class="form-group">
              <label>Application:</label>
              <select [(ngModel)]="newRequest.application" name="application">
                <option value="CRM">CRM</option>
                <option value="Finance Portal">Finance Portal</option>
              </select>
            </div>

            <div class="form-group">
              <label>Environment:</label>
              <select [(ngModel)]="newRequest.environment" name="environment">
                <option value="NonProduction">NonProduction</option>
                <option value="Production">Production (High Risk)</option>
              </select>
            </div>

            <div class="form-group">
              <label>Access Level:</label>
              <select [(ngModel)]="newRequest.accessLevel" name="accessLevel">
                <option value="Read">Read</option>
                <option value="Admin">Admin (High Risk)</option>
              </select>
            </div>

            <div class="form-group">
              <label>Justification:</label>
              <textarea [(ngModel)]="newRequest.justification" name="justification" placeholder="Reason for access..."></textarea>
            </div>

            <button type="submit" class="btn primary" [disabled]="submitting()">
              {{ submitting() ? 'Submitting...' : 'Submit Request' }}
            </button>
          </form>
        </section>

        <section class="card">
          <h2>Access Requests Inbox</h2>
          @if (loading()) {
            <p>Loading requests...</p>
          } @else if (requests().length === 0) {
            <p class="empty">No access requests found.</p>
          } @else {
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Requester</th>
                  <th>App</th>
                  <th>Environment</th>
                  <th>Level</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (req of requests(); track req.id) {
                  <tr>
                    <td>#{{ req.id }}</td>
                    <td>{{ req.requester }}</td>
                    <td>{{ req.application }}</td>
                    <td>{{ req.environment }}</td>
                    <td>{{ req.accessLevel }}</td>
                    <td><span class="badge {{ req.status.toLowerCase() }}">{{ req.status }}</span></td>
                    <td>
                      <button class="btn success" (click)="approve(req)">Approve</button>
                      <button class="btn danger" (click)="rejectPrompt(req)">Reject</button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>
      </main>
    </div>
  `,
  styles: [`
    .app-container { font-family: Arial, sans-serif; max-width: 1000px; margin: 0 auto; padding: 20px; color: #333; }
    .header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #eee; padding-bottom: 10px; margin-bottom: 20px; }
    .user-switcher select { padding: 6px; font-size: 14px; border-radius: 4px; }
    .card { background: #fff; border: 1px solid #ddd; border-radius: 6px; padding: 20px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
    .form-group { margin-bottom: 15px; }
    .form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
    .form-group select, .form-group textarea { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; }
    table { width: 100%; border-collapse: collapse; margin-top: 10px; }
    th, td { padding: 10px; text-align: left; border-bottom: 1px solid #eee; font-size: 14px; }
    .btn { padding: 6px 12px; border: none; border-radius: 4px; cursor: pointer; font-size: 12px; margin-right: 5px; }
    .btn.primary { background: #007bff; color: white; }
    .btn.success { background: #28a745; color: white; }
    .btn.danger { background: #dc3545; color: white; }
    .badge { padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: bold; }
    .badge.pendingmanager { background: #ffc107; color: #333; }
    .badge.pendingsystemowner { background: #17a2b8; color: white; }
    .badge.approved { background: #28a745; color: white; }
    .badge.rejected { background: #dc3545; color: white; }
    .alert { padding: 10px; margin-bottom: 15px; background: #d4edda; color: #155724; border-radius: 4px; }
    .alert.error { background: #f8d7da; color: #721c24; }
  `]
})
export class AppComponent implements OnInit {
  userService = inject(UserService);
  apiService = inject(AccessRequestService);

  requests = signal<AccessRequest[]>([]);
  loading = signal<boolean>(false);
  message = signal<string>('');
  isError = signal<boolean>(false);
  submitting = signal<boolean>(false);

  newRequest = {
    application: 'CRM',
    environment: 'NonProduction',
    accessLevel: 'Read',
    justification: ''
  };

  private clientRequestId = crypto.randomUUID();
  private lastPayloadKey = '';

  get currentActiveUser(): string {
    return this.userService.currentUser();
  }

  ngOnInit() {
    this.loadRequests();
  }

  switchUser(email: string) {
    this.userService.setUser(email);
    this.loadRequests();
  }

  loadRequests() {
    this.loading.set(true);
    this.apiService.getRequests().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.showError(err.error?.message || 'Failed to load requests.');
        this.loading.set(false);
      }
    });
  }

  onSubmit(event: Event) {
  event.preventDefault();

  if (this.submitting()) return;

  if (!this.newRequest.justification.trim()) {
    this.showError('Validation Error: Justification is mandatory.');
    return;
  }

  const payloadKey = JSON.stringify({
    ...this.newRequest,
    justification: this.newRequest.justification.trim()
  });

  if (payloadKey !== this.lastPayloadKey) {
    this.clientRequestId = crypto.randomUUID();
    this.lastPayloadKey = payloadKey;
  }

  const dto = {
    clientRequestId: this.clientRequestId,
    ...this.newRequest
  };

  this.submitting.set(true);
  this.apiService.createRequest(dto).subscribe({
    next: () => {
      this.showSuccess('Access request created successfully!');
      this.newRequest.justification = '';
      this.clientRequestId = crypto.randomUUID();
      this.lastPayloadKey = '';
      this.submitting.set(false);
      this.loadRequests();
    },
    error: (err: any) => {
      this.showError(err.error?.message || 'Failed to create request.');
      this.submitting.set(false);
    }
  });
}

  approve(req: AccessRequest) {
    this.apiService.approveRequest(req.id, req.version).subscribe({
      next: () => {
        this.showSuccess(`Request #${req.id} approved.`);
        this.loadRequests();
      },
      error: (err: any) => {
        this.showError(err.error?.message || 'Action failed.');
      }
    });
  }

  rejectPrompt(req: AccessRequest) {
    const input = prompt('Enter rejection reason (Mandatory):');

    if (input === null) return;

    const reason = input.trim();
    if (!reason) {
      this.showError('Validation Error: Rejection reason is mandatory.');
      return;
    }

    this.apiService.rejectRequest(req.id, req.version, reason).subscribe({
      next: () => {
        this.showSuccess(`Request #${req.id} rejected.`);
        this.loadRequests();
      },
      error: (err: any) => {
        this.showError(err.error?.message || 'Action failed.');
      }
    });
  }

  private showSuccess(msg: string) {
    this.isError.set(false);
    this.message.set(msg);
    setTimeout(() => this.message.set(''), 4000);
  }

  private showError(msg: string) {
    this.isError.set(true);
    this.message.set(msg);
    setTimeout(() => this.message.set(''), 5000);
  }
}
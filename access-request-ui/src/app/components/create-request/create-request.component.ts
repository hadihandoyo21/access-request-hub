import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AccessRequestService } from '../../services/access-request.service';

@Component({
  selector: 'app-create-request',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './create-request.component.html'
})
export class CreateRequestComponent {
  requestForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private requestService: AccessRequestService,
    private router: Router
  ) {
    this.requestForm = this.fb.group({
      application: ['', Validators.required],
      environment: ['Development', Validators.required],
      accessLevel: ['Read', Validators.required],
      justification: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.requestForm.invalid) return;

    // Generate Idempotency Key
    const requestPayload = {
      ...this.requestForm.value,
      clientRequestId: crypto.randomUUID() 
    };

    this.requestService.createRequest(requestPayload).subscribe({
      next: () => this.router.navigate(['/requests']),
      error: (err) => alert('Error creating request: ' + err.message)
    });
  }
}
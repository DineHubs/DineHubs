import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Mail, ArrowLeft, Send, CheckCircle2, AlertCircle } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LucideAngularModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  forgotPasswordForm: FormGroup;
  isLoading = signal(false);
  successMessage = signal('');

  // Icons
  mailIcon = Mail;
  arrowLeftIcon = ArrowLeft;
  sendIcon = Send;
  checkCircleIcon = CheckCircle2;
  alertCircleIcon = AlertCircle;

  constructor() {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.valid) {
      this.isLoading.set(true);
      this.successMessage.set('');

      this.apiService.post<any>('Auth/forgot-password', this.forgotPasswordForm.value).subscribe({
        next: (response) => {
          this.isLoading.set(false);
          const message = response.message || 'If the email exists, a password reset link has been sent.';
          this.successMessage.set(message);
          this.toastService.success(message);
        },
        error: (error) => {
          this.isLoading.set(false);
          const errorMessage = error.error?.message || 'An error occurred. Please try again.';
          this.toastService.error(errorMessage);
        }
      });
    } else {
      Object.keys(this.forgotPasswordForm.controls).forEach(key => {
        this.forgotPasswordForm.get(key)?.markAsTouched();
      });
    }
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  get emailControl() {
    return this.forgotPasswordForm.get('email');
  }
}

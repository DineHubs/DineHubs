import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Mail, Key, Lock, Eye, EyeOff, ArrowLeft, CheckCircle2, AlertCircle } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LucideAngularModule
  ],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastService = inject(ToastService);

  resetPasswordForm: FormGroup;
  isLoading = signal(false);
  hidePassword = signal(true);
  hideConfirmPassword = signal(true);

  // Icons
  mailIcon = Mail;
  keyIcon = Key;
  lockIcon = Lock;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;
  arrowLeftIcon = ArrowLeft;
  checkCircleIcon = CheckCircle2;
  alertCircleIcon = AlertCircle;

  constructor() {
    this.resetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      token: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.resetPasswordForm.patchValue({ email: params['email'] });
      }
      if (params['token']) {
        this.resetPasswordForm.patchValue({ token: params['token'] });
      }
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.valid) {
      this.isLoading.set(true);

      const { email, token, newPassword } = this.resetPasswordForm.value;
      this.apiService.post<any>('Auth/reset-password', { email, token, newPassword }).subscribe({
        next: (response) => {
          this.isLoading.set(false);
          const message = response.message || 'Password has been reset successfully.';
          this.toastService.success(message);
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        },
        error: (error) => {
          this.isLoading.set(false);
          const errorMessage = error.error?.message || 'Failed to reset password. Please check your token and try again.';
          this.toastService.error(errorMessage);
        }
      });
    } else {
      Object.keys(this.resetPasswordForm.controls).forEach(key => {
        this.resetPasswordForm.get(key)?.markAsTouched();
      });
    }
  }

  togglePasswordVisibility(): void {
    this.hidePassword.set(!this.hidePassword());
  }

  toggleConfirmPasswordVisibility(): void {
    this.hideConfirmPassword.set(!this.hideConfirmPassword());
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  get emailControl() {
    return this.resetPasswordForm.get('email');
  }

  get tokenControl() {
    return this.resetPasswordForm.get('token');
  }

  get newPasswordControl() {
    return this.resetPasswordForm.get('newPassword');
  }

  get confirmPasswordControl() {
    return this.resetPasswordForm.get('confirmPassword');
  }
}

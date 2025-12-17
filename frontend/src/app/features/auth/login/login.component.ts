import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Mail, Lock, Eye, EyeOff, LogIn, AlertCircle } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LucideAngularModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private themeService = inject(ThemeService);
  private toastService = inject(ToastService);

  loginForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal('');
  hidePassword = signal(true);

  // Icons
  mailIcon = Mail;
  lockIcon = Lock;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;
  logInIcon = LogIn;
  alertCircleIcon = AlertCircle;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['waiter@dinehub.com', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      this.errorMessage.set('');

      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.toastService.success('Welcome back!');
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading.set(false);
          const message = error.error?.message || 'Invalid email or password';
          this.errorMessage.set(message);
          this.toastService.error(message);
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }

  togglePasswordVisibility(): void {
    this.hidePassword.set(!this.hidePassword());
  }

  goToForgotPassword(): void {
    this.router.navigate(['/forgot-password']);
  }

  get emailControl() {
    return this.loginForm.get('email');
  }

  get passwordControl() {
    return this.loginForm.get('password');
  }
}

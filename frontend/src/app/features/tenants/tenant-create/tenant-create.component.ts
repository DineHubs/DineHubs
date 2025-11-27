import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';

export interface SubscriptionPlan {
  code: number;
  displayName: string;
  monthlyPrice: number;
  annualPrice: number;
  maxBranches: number;
  maxUsers: number;
  includesInventory: boolean;
  includesAdvancedReporting: boolean;
  includesWhatsAppBilling: boolean;
}

@Component({
  selector: 'app-tenant-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './tenant-create.component.html',
  styleUrl: './tenant-create.component.scss'
})
export class TenantCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  tenantForm: FormGroup;
  plans: SubscriptionPlan[] = [];
  isLoading = false;
  isLoadingPlans = false;

  constructor() {
    this.tenantForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      adminEmail: ['', [Validators.required, Validators.email]],
      planCode: ['', [Validators.required]],
      autoRenew: [true]
    });
  }

  ngOnInit(): void {
    this.loadPlans();
  }

  loadPlans(): void {
    this.isLoadingPlans = true;
    this.apiService.get<SubscriptionPlan[]>('Tenants/plans').subscribe({
      next: (data) => {
        this.plans = data;
        this.isLoadingPlans = false;
      },
      error: (err) => {
        console.error('Error loading plans', err);
        this.snackBar.open('Failed to load subscription plans', 'Close', { duration: 3000 });
        this.isLoadingPlans = false;
      }
    });
  }

  onSubmit(): void {
    if (this.tenantForm.valid) {
      this.isLoading = true;
      const formValue = this.tenantForm.value;
      const request = {
        name: formValue.name,
        code: formValue.code,
        adminEmail: formValue.adminEmail,
        planCode: Number(formValue.planCode),
        autoRenew: formValue.autoRenew
      };

      this.apiService.post<any>('Tenants', request).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.snackBar.open('Tenant created successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/tenants']);
        },
        error: (error) => {
          this.isLoading = false;
          const errorMessage = error.error?.message || 'Failed to create tenant';
          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tenants']);
  }
}


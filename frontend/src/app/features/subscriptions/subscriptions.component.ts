import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';

export interface Subscription {
  id: string;
  tenantId: string;
  tenantName: string;
  planCode: string;
  planDisplayName: string;
  status: string;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  billingProvider: string;
  externalSubscriptionId?: string;
}

export interface SubscriptionPlan {
  code: string;
  displayName: string;
  monthlyPrice: number;
  annualPrice: number;
  durationDays?: number;
  maxBranches: number;
  maxUsers: number;
  includesInventory: boolean;
  includesAdvancedReporting: boolean;
  includesWhatsAppBilling: boolean;
}

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatTooltipModule,
    RouterModule
  ],
  templateUrl: './subscriptions.component.html',
  styleUrl: './subscriptions.component.scss'
})
export class SubscriptionsComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  subscriptions: Subscription[] = [];
  plans: SubscriptionPlan[] = [];
  isLoading = true;
  hasError = false;
  errorMessage = '';

  displayedColumns: string[] = ['tenantName', 'plan', 'status', 'startDate', 'endDate', 'autoRenew', 'actions'];

  ngOnInit(): void {
    // Check if user is SuperAdmin - Subscriptions is SuperAdmin only
    if (!this.authService.isSuperAdmin()) {
      this.hasError = true;
      this.errorMessage = 'Access Denied: Subscriptions management is only accessible to Super Administrators.';
      this.isLoading = false;
      return;
    }

    this.loadSubscriptions();
    this.loadPlans();
  }

  loadSubscriptions(): void {
    this.isLoading = true;
    this.hasError = false;

    this.apiService.get<Subscription[]>('Subscriptions').subscribe({
      next: (data) => {
        this.subscriptions = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        this.hasError = true;
        this.errorMessage = error.error?.message || 'Failed to load subscriptions';
        this.isLoading = false;
        this.snackBar.open('Failed to load subscriptions', 'Close', { duration: 3000 });
      }
    });
  }

  loadPlans(): void {
    this.apiService.get<SubscriptionPlan[]>('Subscriptions/plans').subscribe({
      next: (data) => {
        this.plans = data;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.snackBar.open('Failed to load subscription plans', 'Close', { duration: 3000 });
      }
    });
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'primary';
      case 'pending':
        return 'accent';
      case 'cancelled':
      case 'suspended':
        return 'warn';
      default:
        return 'accent';
    }
  }

  formatDate(date: string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString();
  }

  viewTenant(tenantId: string): void {
    // Navigate to tenant details
    window.location.href = `/tenants/${tenantId}`;
  }

  upgradePlan(tenantId: string): void {
    // This would open a dialog to select a new plan
    // For now, just show a message
    this.snackBar.open('Plan upgrade functionality coming soon', 'Close', { duration: 3000 });
  }
}


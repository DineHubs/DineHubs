import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { Tenant } from '../tenant-list/tenant-list.component';

export interface TenantDetail extends Tenant {
  branchesCount: number;
  usersCount: number;
  subscription?: SubscriptionDetail;
}

export interface SubscriptionDetail {
  id: string;
  status: string;
  planCode: string;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  billingProvider: string;
  externalSubscriptionId?: string;
}

export interface TenantUser {
  id: string;
  email: string;
  role: string;
  branchId: string | null;
  isActive: boolean;
}

@Component({
  selector: 'app-tenant-details',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatChipsModule,
    MatSnackBarModule
  ],
  templateUrl: './tenant-details.component.html',
  styleUrl: './tenant-details.component.scss'
})
export class TenantDetailsComponent implements OnInit {
  private apiService = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  tenantId: string = '';
  tenant: TenantDetail | null = null;
  usersDataSource = new MatTableDataSource<TenantUser>([]);
  usersDisplayedColumns = ['email', 'role', 'branchId', 'isActive'];
  isLoading = false;
  isLoadingUsers = false;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.tenantId = params['id'];
      this.loadTenant();
      this.loadUsers();
    });
  }

  loadTenant(): void {
    this.isLoading = true;
    this.apiService.get<TenantDetail>(`Tenants/${this.tenantId}`).subscribe({
      next: (tenant) => {
        this.tenant = tenant;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading tenant', err);
        this.snackBar.open('Failed to load tenant', 'Close', { duration: 3000 });
        this.isLoading = false;
        this.router.navigate(['/tenants']);
      }
    });
  }

  loadUsers(): void {
    this.isLoadingUsers = true;
    this.apiService.get<TenantUser[]>(`Tenants/${this.tenantId}/users`).subscribe({
      next: (users) => {
        this.usersDataSource.data = users;
        this.isLoadingUsers = false;
      },
      error: (err) => {
        console.error('Error loading users', err);
        this.snackBar.open('Failed to load users', 'Close', { duration: 3000 });
        this.isLoadingUsers = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/tenants']);
  }

  getRoleColor(role: string): string {
    const roleColors: { [key: string]: string } = {
      'SuperAdmin': 'warn',
      'Admin': 'primary',
      'Manager': 'accent',
      'Waiter': '',
      'Kitchen': '',
      'InventoryManager': ''
    };
    return roleColors[role] || '';
  }

  getSubscriptionStatusColor(status?: string): string {
    switch (status) {
      case 'Active':
        return 'primary';
      case 'Pending':
        return 'accent';
      case 'Expired':
        return 'warn';
      case 'Cancelled':
      case 'Suspended':
        return '';
      default:
        return '';
    }
  }

  formatDate(dateString?: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString();
  }
}


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
  tenant: Tenant | null = null;
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
    // Since we don't have a GET by ID endpoint, we'll load from list
    // In a real scenario, you'd have GET /api/v1/Tenants/{id}
    this.isLoading = true;
    this.apiService.get<Tenant[]>('Tenants').subscribe({
      next: (tenants) => {
        this.tenant = tenants.find(t => t.id === this.tenantId) || null;
        this.isLoading = false;
        if (!this.tenant) {
          this.snackBar.open('Tenant not found', 'Close', { duration: 3000 });
          this.router.navigate(['/tenants']);
        }
      },
      error: (err) => {
        console.error('Error loading tenant', err);
        this.snackBar.open('Failed to load tenant', 'Close', { duration: 3000 });
        this.isLoading = false;
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
}


import { Component, OnInit, AfterViewInit, ViewChild, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../../core/services/api.service';

export interface Tenant {
  id: string;
  name: string;
  code: string;
  countryCode: string;
  defaultCurrency: string;
  isActive: boolean;
  createdAt: string;
  subscriptionStatus?: string;
  subscriptionPlanCode?: string;
  subscriptionStartDate?: string;
  subscriptionEndDate?: string;
  subscriptionAutoRenew?: boolean;
}

@Component({
  selector: 'app-tenant-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './tenant-list.component.html',
  styleUrl: './tenant-list.component.scss'
})
export class TenantListComponent implements OnInit, AfterViewInit {
  private apiService = inject(ApiService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  dataSource = new MatTableDataSource<Tenant>([]);
  displayedColumns = ['name', 'code', 'subscription', 'countryCode', 'defaultCurrency', 'isActive', 'actions'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.loadTenants();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadTenants(): void {
    this.apiService.get<Tenant[]>('Tenants').subscribe({
      next: (data) => {
        this.dataSource.data = data;
      },
      error: (err) => {
        console.error('Error loading tenants', err);
        this.snackBar.open('Failed to load tenants', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  createTenant(): void {
    this.router.navigate(['/tenants/create']);
  }

  viewTenant(tenant: Tenant): void {
    this.router.navigate(['/tenants', tenant.id]);
  }

  getStatusColor(status?: string): string {
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


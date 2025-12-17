import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AppRoles } from '../../../core/constants/roles.constants';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  orders: Order[] = [];
  isLoading = false;
  displayedColumns = ['orderNumber', 'tableNumber', 'status', 'total', 'createdAt', 'actions'];

  ngOnInit(): void {
    // Access control handled by route guard
    this.loadOrders();
    // Refresh when navigating back to this page
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      if (event.url === '/orders' || event.urlAfterRedirects === '/orders') {
        this.loadOrders();
      }
    });
  }

  loadOrders(): void {
    this.isLoading = true;
    this.apiService.get<Order[]>('Orders').subscribe({
      next: (orders) => {
        this.orders = orders;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        this.snackBar.open('Failed to load orders', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  getStatusText(status: number | OrderStatus): string {
    const statusMap: Record<number, string> = {
      [OrderStatus.Draft]: 'Draft',
      [OrderStatus.Submitted]: 'Submitted',
      [OrderStatus.InPreparation]: 'In Preparation',
      [OrderStatus.Ready]: 'Ready',
      [OrderStatus.Delivered]: 'Delivered',
      [OrderStatus.Cancelled]: 'Cancelled',
      [OrderStatus.Paid]: 'Paid'
    };
    return statusMap[status] || 'Unknown';
  }

  getStatusColor(status: number | OrderStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const colors: Record<number, string> = {
      [OrderStatus.Draft]: '',
      [OrderStatus.Submitted]: 'primary',
      [OrderStatus.InPreparation]: 'accent',
      [OrderStatus.Ready]: 'warn',
      [OrderStatus.Delivered]: '',
      [OrderStatus.Cancelled]: 'warn',
      [OrderStatus.Paid]: 'primary'
    };
    return colors[statusNum] || '';
  }
}


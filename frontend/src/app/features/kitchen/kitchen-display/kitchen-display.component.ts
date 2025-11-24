import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-kitchen-display',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './kitchen-display.component.html',
  styleUrl: './kitchen-display.component.scss'
})
export class KitchenDisplayComponent implements OnInit, OnDestroy {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);
  private refreshSubscription?: Subscription;

  orders: Order[] = [];
  isLoading = false;
  OrderStatus = OrderStatus; // Expose enum to template
  
  ngOnInit(): void {
    // Check if user has required role
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.authService.hasAnyRole(['SuperAdmin', 'Kitchen', 'Manager'])) {
      this.snackBar.open('You do not have permission to access this page', 'Close', { duration: 5000 });
      this.router.navigate(['/dashboard']);
      return;
    }

    this.loadOrders();
    // Auto-refresh every 5 seconds
    this.refreshSubscription = interval(5000).subscribe(() => {
      this.loadOrders();
    });
  }

  ngOnDestroy(): void {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }

  loadOrders(): void {
    this.isLoading = true;
    this.apiService.get<Order[]>('Kitchen/queue').subscribe({
      next: (orders) => {
        this.orders = orders;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading kitchen orders:', error);
        if (!this.orders.length) {
          this.snackBar.open('Failed to load kitchen orders', 'Close', { duration: 3000 });
        }
        this.isLoading = false;
      }
    });
  }

  updateOrderStatus(orderId: string, status: OrderStatus): void {
    this.apiService.patch(`Orders/${orderId}/status?status=${status}`, {}).subscribe({
      next: () => {
        this.loadOrders();
        this.snackBar.open('Order status updated', 'Close', { duration: 2000 });
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        this.snackBar.open('Failed to update order status', 'Close', { duration: 3000 });
      }
    });
  }

  getStatusColor(status: OrderStatus): string {
    const colors: Record<OrderStatus, string> = {
      [OrderStatus.Submitted]: 'primary',
      [OrderStatus.InPreparation]: 'accent',
      [OrderStatus.Ready]: 'warn',
      [OrderStatus.Served]: '',
      [OrderStatus.Completed]: '',
      [OrderStatus.Cancelled]: 'warn'
    };
    return colors[status] || '';
  }
}


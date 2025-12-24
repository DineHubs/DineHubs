import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, RefreshCw, ChefHat, Play, CheckCircle2, Clock, AlertCircle, CreditCard } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { interval, Subscription } from 'rxjs';

interface CanSubmitToKitchenResult {
  canSubmit: boolean;
  requiresPayment: boolean;
  paymentStatus: string | null;
  message: string | null;
}

@Component({
  selector: 'app-kitchen-display',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    LucideAngularModule
  ],
  templateUrl: './kitchen-display.component.html',
  styleUrl: './kitchen-display.component.scss'
})
export class KitchenDisplayComponent implements OnInit, OnDestroy {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private refreshSubscription?: Subscription;

  orders = signal<Order[]>([]);
  isLoading = signal<boolean>(false);
  OrderStatus = OrderStatus; // Expose enum to template

  // Icons
  refreshIcon = RefreshCw;
  chefHatIcon = ChefHat;
  playIcon = Play;
  checkCircleIcon = CheckCircle2;
  clockIcon = Clock;
  alertCircleIcon = AlertCircle;
  creditCardIcon = CreditCard;

  // Payment warning state
  showPaymentWarningModal = signal<boolean>(false);
  paymentWarningOrder = signal<Order | null>(null);
  paymentWarningMessage = signal<string>('');

  ngOnInit(): void {
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
    this.isLoading.set(true);
    this.apiService.get<Order[]>('Kitchen/queue').subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading kitchen orders:', error);
        if (this.orders().length === 0) {
          this.toastService.error('Failed to load kitchen orders');
        }
        this.isLoading.set(false);
      }
    });
  }

  updateOrderStatus(orderId: string, status: OrderStatus, order?: Order): void {
    // For InPreparation, check payment requirement first
    if (status === OrderStatus.InPreparation) {
      this.apiService.get<CanSubmitToKitchenResult>(`Orders/${orderId}/can-submit-to-kitchen`).subscribe({
        next: (result) => {
          if (result.canSubmit) {
            this.performStatusUpdate(orderId, status);
          } else {
            // Show payment warning modal
            this.paymentWarningMessage.set(result.message || 'Payment is required before starting preparation');
            this.paymentWarningOrder.set(order || null);
            this.showPaymentWarningModal.set(true);
          }
        },
        error: (error) => {
          console.error('Error checking kitchen submission eligibility:', error);
          // Proceed with update anyway - let backend handle validation
          this.performStatusUpdate(orderId, status);
        }
      });
    } else {
      this.performStatusUpdate(orderId, status);
    }
  }

  private performStatusUpdate(orderId: string, status: OrderStatus): void {
    this.apiService.patch(`Orders/${orderId}/status?status=${status}`, {}).subscribe({
      next: () => {
        this.loadOrders();
        this.toastService.success('Order status updated');
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        const errorMessage = error.error?.message || error.error?.Message || 'Failed to update order status';
        this.toastService.error(errorMessage);
      }
    });
  }

  closePaymentWarningModal(): void {
    this.showPaymentWarningModal.set(false);
    this.paymentWarningOrder.set(null);
    this.paymentWarningMessage.set('');
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

  getStatusIcon(status: number | OrderStatus) {
    const icons: Record<number, any> = {
      [OrderStatus.Submitted]: this.clockIcon,
      [OrderStatus.InPreparation]: this.playIcon,
      [OrderStatus.Ready]: this.checkCircleIcon
    };
    return icons[status] || this.alertCircleIcon;
  }

  getStatusBadgeClasses(status: number | OrderStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const classes: Record<number, string> = {
      [OrderStatus.Submitted]: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-700',
      [OrderStatus.InPreparation]: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-700',
      [OrderStatus.Ready]: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-700'
    };
    return classes[statusNum] || 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300';
  }

  getCardBorderClasses(status: number | OrderStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const classes: Record<number, string> = {
      [OrderStatus.Submitted]: 'border-l-4 border-l-blue-500',
      [OrderStatus.InPreparation]: 'border-l-4 border-l-orange-500',
      [OrderStatus.Ready]: 'border-l-4 border-l-green-500'
    };
    return classes[statusNum] || '';
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { LucideAngularModule, ArrowLeft, CreditCard, CheckCircle2, XCircle, Clock, AlertCircle } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order, Payment, ProcessPaymentRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-payment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    LucideAngularModule
  ],
  templateUrl: './order-payment.component.html',
  styleUrl: './order-payment.component.scss'
})
export class OrderPaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  router = inject(Router);
  private toastService = inject(ToastService);

  orderId: string = '';
  orderTotal: number = 0;
  order: Order | null = null;

  paymentProviders = ['Stripe', 'IPay88', 'FPX'];
  selectedProvider = 'Stripe';
  amount: number = 0;
  isLoading = false;
  payment: Payment | null = null;

  // Icons
  arrowLeftIcon = ArrowLeft;
  creditCardIcon = CreditCard;
  checkCircleIcon = CheckCircle2;
  xCircleIcon = XCircle;
  clockIcon = Clock;
  alertCircleIcon = AlertCircle;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.orderId = id;
      this.loadOrder();
    }
  }

  loadOrder(): void {
    this.apiService.get<Order>(`Orders/${this.orderId}`).subscribe({
      next: (order) => {
        this.order = order;
        this.orderTotal = order.total;
        this.amount = order.total;
        this.loadPayment();
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.toastService.error('Failed to load order');
        this.router.navigate(['/orders']);
      }
    });
  }

  loadPayment(): void {
    this.apiService.get<Payment>(`Payments/orders/${this.orderId}`).subscribe({
      next: (payment) => {
        this.payment = payment;
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error loading payment:', error);
        }
      }
    });
  }

  processPayment(): void {
    if (this.amount <= 0) {
      this.toastService.error('Payment amount must be greater than zero');
      return;
    }

    if (this.amount > this.orderTotal) {
      this.toastService.error('Payment amount cannot exceed order total');
      return;
    }

    this.isLoading = true;

    const request: ProcessPaymentRequest = {
      amount: this.amount,
      provider: this.selectedProvider,
      metadata: {
        orderId: this.orderId
      }
    };

    this.apiService.post<Payment>(`Payments/orders/${this.orderId}/pay`, request).subscribe({
      next: (payment) => {
        this.payment = payment;
        this.toastService.success('Payment processed successfully');
        this.isLoading = false;
        this.router.navigate(['/orders', this.orderId]);
      },
      error: (error) => {
        console.error('Error processing payment:', error);
        const errorMessage = error.error?.message || 'Failed to process payment';
        this.toastService.error(errorMessage);
        this.isLoading = false;
      }
    });
  }

  getPaymentStatusText(status: number): string {
    const statusMap: Record<number, string> = {
      1: 'Pending',
      2: 'Authorized',
      3: 'Captured',
      4: 'Failed',
      5: 'Refunded',
      6: 'Voided'
    };
    return statusMap[status] || 'Unknown';
  }

  getPaymentStatusIcon(status: number) {
    const icons: Record<number, any> = {
      1: this.clockIcon,
      2: this.checkCircleIcon,
      3: this.checkCircleIcon,
      4: this.xCircleIcon,
      5: this.alertCircleIcon,
      6: this.xCircleIcon
    };
    return icons[status] || this.clockIcon;
  }

  getPaymentStatusBadgeClasses(status: number): string {
    const classes: Record<number, string> = {
      1: 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-300 border-yellow-200 dark:border-yellow-700',
      2: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-700',
      3: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-700',
      4: 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 border-red-200 dark:border-red-700',
      5: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-700',
      6: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700'
    };
    return classes[status] || 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300';
  }
}

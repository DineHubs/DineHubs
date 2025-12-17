import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Order, Payment, ProcessPaymentRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-payment',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    FormsModule
  ],
  templateUrl: './order-payment.component.html',
  styleUrl: './order-payment.component.scss'
})
export class OrderPaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  router = inject(Router);
  private snackBar = inject(MatSnackBar);

  orderId: string = '';
  orderTotal: number = 0;
  order: Order | null = null;

  paymentProviders = ['Stripe', 'IPay88', 'FPX'];
  selectedProvider = 'Stripe';
  amount: number = 0;
  isLoading = false;
  payment: Payment | null = null;

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
        this.snackBar.open('Failed to load order', 'Close', { duration: 3000 });
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
        // Payment might not exist yet, which is fine
        if (error.status !== 404) {
          console.error('Error loading payment:', error);
        }
      }
    });
  }

  processPayment(): void {
    if (this.amount <= 0) {
      this.snackBar.open('Payment amount must be greater than zero', 'Close', { duration: 3000 });
      return;
    }

    if (this.amount > this.orderTotal) {
      this.snackBar.open('Payment amount cannot exceed order total', 'Close', { duration: 3000 });
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
        this.snackBar.open('Payment processed successfully', 'Close', { duration: 3000 });
        this.isLoading = false;
        // Navigate back to order details or orders list
        this.router.navigate(['/orders', this.orderId]);
      },
      error: (error) => {
        console.error('Error processing payment:', error);
        const errorMessage = error.error?.message || 'Failed to process payment';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
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
}


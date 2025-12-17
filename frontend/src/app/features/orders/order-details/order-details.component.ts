import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Order, OrderStatus, CancelOrderRequest, UpdateOrderLineRequest, Payment, ProcessPaymentRequest, ReprintReceiptRequest } from '../../../core/models/order.model';
import { CancelOrderDialogComponent } from './cancel-order-dialog.component';
import { ReprintReceiptDialogComponent } from './reprint-receipt-dialog.component';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    FormsModule
  ],
  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.scss'
})
export class OrderDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  order: Order | null = null;
  payment: Payment | null = null;
  receiptUrl: string | null = null;

  ngOnInit(): void {
    // Access control handled by route guard
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'active') {
      this.loadOrder(id);
    } else if (id === 'active') {
      // Redirect to orders list if "active" is used as ID
      this.router.navigate(['/orders']);
    }
  }

  loadOrder(id: string): void {
    this.apiService.get<Order>(`Orders/${id}`).subscribe({
      next: (order) => {
        this.order = order;
        this.loadPayment(id);
        this.loadReceipt(id);
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.snackBar.open('Failed to load order', 'Close', { duration: 3000 });
      }
    });
  }

  loadPayment(orderId: string): void {
    this.apiService.get<Payment>(`Payments/orders/${orderId}`).subscribe({
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

  loadReceipt(orderId: string): void {
    this.apiService.get<{ receiptUrl: string }>(`Receipts/orders/${orderId}`).subscribe({
      next: (response) => {
        this.receiptUrl = response.receiptUrl;
      },
      error: (error) => {
        // Receipt might not exist yet, which is fine
        if (error.status !== 404) {
          console.error('Error loading receipt:', error);
        }
      }
    });
  }

  canCancel(): boolean {
    if (!this.order) return false;
    const status = typeof this.order.status === 'number' ? this.order.status : this.order.status;
    return status < OrderStatus.InPreparation && status !== OrderStatus.Cancelled && status !== OrderStatus.Paid;
  }

  canModifyLines(): boolean {
    if (!this.order) return false;
    const status = typeof this.order.status === 'number' ? this.order.status : this.order.status;
    return status < OrderStatus.InPreparation;
  }

  cancelOrder(): void {
    if (!this.order) return;

    const dialogRef = this.dialog.open(CancelOrderDialogComponent, {
      width: '400px',
      data: { orderNumber: this.order.orderNumber }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.reason) {
        const request: CancelOrderRequest = { reason: result.reason };
        this.apiService.post(`Orders/${this.order!.id}/cancel`, request).subscribe({
          next: () => {
            this.snackBar.open('Order cancelled successfully', 'Close', { duration: 3000 });
            this.loadOrder(this.order!.id);
            this.router.navigate(['/orders']);
          },
          error: (error) => {
            console.error('Error cancelling order:', error);
            const errorMessage = error.error?.message || 'Failed to cancel order';
            this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  removeLine(lineId: string): void {
    if (!this.order || !lineId) return;

    if (!confirm('Are you sure you want to remove this item from the order?')) {
      return;
    }

    this.apiService.delete(`Orders/${this.order.id}/lines/${lineId}`).subscribe({
      next: () => {
        this.snackBar.open('Item removed successfully', 'Close', { duration: 3000 });
        this.loadOrder(this.order!.id);
      },
      error: (error) => {
        console.error('Error removing line:', error);
        const errorMessage = error.error?.message || 'Failed to remove item';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  updateLineQuantity(lineId: string, newQuantity: number): void {
    if (!this.order || !lineId || newQuantity <= 0) return;

    const request: UpdateOrderLineRequest = { quantity: newQuantity };
    this.apiService.patch(`Orders/${this.order.id}/lines/${lineId}`, request).subscribe({
      next: () => {
        this.snackBar.open('Quantity updated successfully', 'Close', { duration: 3000 });
        this.loadOrder(this.order!.id);
      },
      error: (error) => {
        console.error('Error updating quantity:', error);
        const errorMessage = error.error?.message || 'Failed to update quantity';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  viewReceipt(): void {
    if (!this.receiptUrl) {
      this.snackBar.open('Receipt not available', 'Close', { duration: 3000 });
      return;
    }
    window.open(this.receiptUrl, '_blank');
  }

  reprintReceipt(): void {
    if (!this.order) return;

    const dialogRef = this.dialog.open(ReprintReceiptDialogComponent, {
      width: '400px',
      data: { orderNumber: this.order.orderNumber }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.reason) {
        const request: ReprintReceiptRequest = { reason: result.reason };
        this.apiService.post<{ receiptUrl: string }>(`Receipts/orders/${this.order!.id}/reprint`, request).subscribe({
          next: (response) => {
            this.receiptUrl = response.receiptUrl;
            this.snackBar.open('Receipt reprinted successfully', 'Close', { duration: 3000 });
            window.open(response.receiptUrl, '_blank');
          },
          error: (error) => {
            console.error('Error reprinting receipt:', error);
            const errorMessage = error.error?.message || 'Failed to reprint receipt';
            this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          }
        });
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

  get OrderStatus() {
    return OrderStatus;
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


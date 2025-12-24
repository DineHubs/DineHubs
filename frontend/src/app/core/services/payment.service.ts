import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface PaymentTransaction {
  id: string;
  orderId: string;
  amount: number;
  provider: string;
  status: PaymentStatus;
  transactionReference?: string;
  metadata?: Record<string, string>;
  createdAt: string;
  updatedAt?: string;
}

export enum PaymentStatus {
  Pending = 1,
  Authorized = 2,
  Captured = 3,
  Failed = 4,
  Refunded = 5,
  Voided = 6
}

export interface ProcessPaymentRequest {
  amount: number;
  provider: string;
  metadata?: Record<string, string>;
}

export interface RefundPaymentRequest {
  amount: number;
  reason: string;
}

export interface VoidPaymentRequest {
  reason: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiService = inject(ApiService);

  /**
   * Process payment for an order
   */
  processPayment(orderId: string, request: ProcessPaymentRequest): Observable<PaymentTransaction> {
    return this.apiService.post<PaymentTransaction>(`Payments/orders/${orderId}/pay`, request);
  }

  /**
   * Get payment by order ID
   */
  getPaymentByOrderId(orderId: string): Observable<PaymentTransaction | null> {
    return this.apiService.get<PaymentTransaction | null>(`Payments/orders/${orderId}`);
  }

  /**
   * Refund a payment (full or partial)
   */
  refundPayment(paymentId: string, request: RefundPaymentRequest): Observable<PaymentTransaction> {
    return this.apiService.post<PaymentTransaction>(`Payments/${paymentId}/refund`, request);
  }

  /**
   * Void a payment (cancel before capture)
   */
  voidPayment(paymentId: string, request: VoidPaymentRequest): Observable<PaymentTransaction> {
    return this.apiService.post<PaymentTransaction>(`Payments/${paymentId}/void`, request);
  }

  /**
   * Check if payment can be voided (status is Authorized)
   */
  canVoid(payment: PaymentTransaction): boolean {
    return payment.status === PaymentStatus.Authorized;
  }

  /**
   * Check if payment can be refunded (status is Captured)
   */
  canRefund(payment: PaymentTransaction): boolean {
    return payment.status === PaymentStatus.Captured;
  }

  /**
   * Get payment status display text
   */
  getStatusText(status: PaymentStatus): string {
    switch (status) {
      case PaymentStatus.Pending: return 'Pending';
      case PaymentStatus.Authorized: return 'Authorized';
      case PaymentStatus.Captured: return 'Captured';
      case PaymentStatus.Failed: return 'Failed';
      case PaymentStatus.Refunded: return 'Refunded';
      case PaymentStatus.Voided: return 'Voided';
      default: return 'Unknown';
    }
  }
}


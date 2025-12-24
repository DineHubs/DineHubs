import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { LucideAngularModule, ArrowLeft, X, Trash2, CreditCard, Receipt, Printer, FileText, CheckCircle2, ChefHat, Clock, Truck, XCircle, Plus, Minus, AlertCircle, RotateCcw, Ban, Send } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { PrintService } from '../../../core/services/print.service';
import { PaymentService, PaymentStatus, PaymentTransaction } from '../../../core/services/payment.service';
import { KitchenPrintService } from '../../../core/services/kitchen-print.service';
import { Order, OrderStatus, CancelOrderRequest, UpdateOrderLineRequest, Payment, ReprintReceiptRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LucideAngularModule
  ],
  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.scss'
})
export class OrderDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private printService = inject(PrintService);
  private paymentService = inject(PaymentService);
  private kitchenPrintService = inject(KitchenPrintService);

  order = signal<Order | null>(null);
  payment = signal<Payment | null>(null);
  paymentTransaction = signal<PaymentTransaction | null>(null);
  receiptUrl = signal<string | null>(null);

  // Modal states
  showCancelModal = signal<boolean>(false);
  showReprintModal = signal<boolean>(false);
  showRefundModal = signal<boolean>(false);
  showVoidModal = signal<boolean>(false);
  showKitchenReprintModal = signal<boolean>(false);
  cancelReason = signal<string>('');
  reprintReason = signal<string>('');
  refundReason = signal<string>('');
  refundAmount = signal<number>(0);
  refundType = signal<'full' | 'partial'>('full');
  voidReason = signal<string>('');
  kitchenReprintReason = signal<string>('');

  // Icons
  arrowLeftIcon = ArrowLeft;
  xIcon = X;
  trashIcon = Trash2;
  creditCardIcon = CreditCard;
  receiptIcon = Receipt;
  printerIcon = Printer;
  fileTextIcon = FileText;
  checkCircleIcon = CheckCircle2;
  chefHatIcon = ChefHat;
  clockIcon = Clock;
  truckIcon = Truck;
  xCircleIcon = XCircle;
  plusIcon = Plus;
  minusIcon = Minus;
  alertCircleIcon = AlertCircle;
  refundIcon = RotateCcw;
  voidIcon = Ban;
  sendIcon = Send;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'active') {
      this.loadOrder(id);
    } else if (id === 'active') {
      this.router.navigate(['/orders']);
    }
  }

  loadOrder(id: string): void {
    this.apiService.get<Order>(`Orders/${id}`).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loadPayment(id);
        this.loadReceipt(id);
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.toastService.error('Failed to load order');
      }
    });
  }

  loadPayment(orderId: string): void {
    this.apiService.get<Payment>(`Payments/orders/${orderId}`).subscribe({
      next: (payment) => {
        this.payment.set(payment);
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error loading payment:', error);
        }
      }
    });

    // Also load payment transaction for void/refund status
    this.paymentService.getPaymentByOrderId(orderId).subscribe({
      next: (transaction) => {
        this.paymentTransaction.set(transaction);
        if (transaction) {
          this.refundAmount.set(transaction.amount);
        }
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error loading payment transaction:', error);
        }
      }
    });
  }

  loadReceipt(orderId: string): void {
    this.apiService.get<{ receiptUrl: string }>(`Receipts/orders/${orderId}`).subscribe({
      next: (response) => {
        this.receiptUrl.set(response.receiptUrl);
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error loading receipt:', error);
        }
      }
    });
  }

  canCancel(): boolean {
    const order = this.order();
    if (!order) return false;
    const status = typeof order.status === 'number' ? order.status : order.status;
    return status < OrderStatus.InPreparation && status !== OrderStatus.Cancelled && status !== OrderStatus.Paid;
  }

  canModifyLines(): boolean {
    const order = this.order();
    if (!order) return false;
    const status = typeof order.status === 'number' ? order.status : order.status;
    return status < OrderStatus.InPreparation;
  }

  openCancelModal(): void {
    this.cancelReason.set('');
    this.showCancelModal.set(true);
  }

  closeCancelModal(): void {
    this.showCancelModal.set(false);
    this.cancelReason.set('');
  }

  confirmCancel(): void {
    const order = this.order();
    if (!order || !this.cancelReason().trim()) return;

    const request: CancelOrderRequest = { reason: this.cancelReason().trim() };
    this.apiService.post(`Orders/${order.id}/cancel`, request).subscribe({
      next: () => {
        this.toastService.success('Order cancelled successfully');
        this.closeCancelModal();
        this.loadOrder(order.id);
        setTimeout(() => {
          this.router.navigate(['/orders']);
        }, 1000);
      },
      error: (error) => {
        console.error('Error cancelling order:', error);
        const errorMessage = error.error?.message || 'Failed to cancel order';
        this.toastService.error(errorMessage);
      }
    });
  }

  removeLine(lineId: string): void {
    const order = this.order();
    if (!order || !lineId) return;

    if (!confirm('Are you sure you want to remove this item from the order?')) {
      return;
    }

    this.apiService.delete(`Orders/${order.id}/lines/${lineId}`).subscribe({
      next: () => {
        this.toastService.success('Item removed successfully');
        this.loadOrder(order.id);
      },
      error: (error) => {
        console.error('Error removing line:', error);
        const errorMessage = error.error?.message || 'Failed to remove item';
        this.toastService.error(errorMessage);
      }
    });
  }

  updateLineQuantity(lineId: string, newQuantity: number): void {
    const order = this.order();
    if (!order || !lineId || newQuantity <= 0) return;

    const request: UpdateOrderLineRequest = { quantity: newQuantity };
    this.apiService.patch(`Orders/${order.id}/lines/${lineId}`, request).subscribe({
      next: () => {
        this.toastService.success('Quantity updated successfully');
        this.loadOrder(order.id);
      },
      error: (error) => {
        console.error('Error updating quantity:', error);
        const errorMessage = error.error?.message || 'Failed to update quantity';
        this.toastService.error(errorMessage);
      }
    });
  }

  viewReceipt(): void {
    const url = this.receiptUrl();
    if (!url) {
      this.toastService.error('Receipt not available');
      return;
    }
    window.open(url, '_blank');
  }

  openReprintModal(): void {
    this.reprintReason.set('');
    this.showReprintModal.set(true);
  }

  closeReprintModal(): void {
    this.showReprintModal.set(false);
    this.reprintReason.set('');
  }

  confirmReprint(): void {
    const order = this.order();
    if (!order || !this.reprintReason().trim()) return;

    const request: ReprintReceiptRequest = { reason: this.reprintReason().trim() };
    this.apiService.post<{ receiptUrl: string }>(`Receipts/orders/${order.id}/reprint`, request).subscribe({
      next: (response) => {
        this.receiptUrl.set(response.receiptUrl);
        this.toastService.success('Receipt reprinted successfully');
        this.closeReprintModal();
        this.printService.printOrder(this.order()!);
      },
      error: (error) => {
        console.error('Error reprinting receipt:', error);
        const errorMessage = error.error?.message || 'Failed to reprint receipt';
        this.toastService.error(errorMessage);
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

  getStatusIcon(status: number | OrderStatus) {
    const statusNum = typeof status === 'number' ? status : status;
    const icons: Record<number, any> = {
      [OrderStatus.Draft]: this.fileTextIcon,
      [OrderStatus.Submitted]: this.checkCircleIcon,
      [OrderStatus.InPreparation]: this.chefHatIcon,
      [OrderStatus.Ready]: this.clockIcon,
      [OrderStatus.Delivered]: this.truckIcon,
      [OrderStatus.Cancelled]: this.xCircleIcon,
      [OrderStatus.Paid]: this.creditCardIcon
    };
    return icons[statusNum] || this.fileTextIcon;
  }

  getStatusBadgeClasses(status: number | OrderStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const classes: Record<number, string> = {
      [OrderStatus.Draft]: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700',
      [OrderStatus.Submitted]: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-700',
      [OrderStatus.InPreparation]: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-700',
      [OrderStatus.Ready]: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-700',
      [OrderStatus.Delivered]: 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-700',
      [OrderStatus.Cancelled]: 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 border-red-200 dark:border-red-700',
      [OrderStatus.Paid]: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-700'
    };
    return classes[statusNum] || 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300';
  }

  get OrderStatus() {
    return OrderStatus;
  }

  get PaymentStatus() {
    return PaymentStatus;
  }

  // Void/Refund Methods
  canVoid(): boolean {
    const transaction = this.paymentTransaction();
    return transaction ? this.paymentService.canVoid(transaction) : false;
  }

  canRefund(): boolean {
    const transaction = this.paymentTransaction();
    return transaction ? this.paymentService.canRefund(transaction) : false;
  }

  openVoidModal(): void {
    this.voidReason.set('');
    this.showVoidModal.set(true);
  }

  closeVoidModal(): void {
    this.showVoidModal.set(false);
    this.voidReason.set('');
  }

  confirmVoid(): void {
    const order = this.order();
    const transaction = this.paymentTransaction();
    if (!order || !transaction || !this.voidReason().trim()) return;

    this.paymentService.voidPayment(transaction.id, { reason: this.voidReason().trim() }).subscribe({
      next: () => {
        this.toastService.success('Payment voided successfully');
        this.closeVoidModal();
        this.loadOrder(order.id);
      },
      error: (error) => {
        console.error('Error voiding payment:', error);
        const errorMessage = error.error?.message || 'Failed to void payment';
        this.toastService.error(errorMessage);
      }
    });
  }

  openRefundModal(): void {
    this.refundReason.set('');
    this.refundType.set('full');
    const transaction = this.paymentTransaction();
    if (transaction) {
      this.refundAmount.set(transaction.amount);
    }
    this.showRefundModal.set(true);
  }

  closeRefundModal(): void {
    this.showRefundModal.set(false);
    this.refundReason.set('');
    this.refundType.set('full');
  }

  isRefundValid(): boolean {
    if (!this.refundReason().trim()) return false;
    const transaction = this.paymentTransaction();
    if (!transaction) return false;
    if (this.refundType() === 'partial') {
      return this.refundAmount() > 0 && this.refundAmount() <= transaction.amount;
    }
    return true;
  }

  confirmRefund(): void {
    const order = this.order();
    const transaction = this.paymentTransaction();
    if (!order || !transaction || !this.isRefundValid()) return;

    const amount = this.refundType() === 'full' ? transaction.amount : this.refundAmount();
    
    this.paymentService.refundPayment(transaction.id, { 
      amount, 
      reason: this.refundReason().trim() 
    }).subscribe({
      next: () => {
        this.toastService.success('Payment refunded successfully');
        this.closeRefundModal();
        this.loadOrder(order.id);
      },
      error: (error) => {
        console.error('Error refunding payment:', error);
        const errorMessage = error.error?.message || 'Failed to process refund';
        this.toastService.error(errorMessage);
      }
    });
  }

  getPaymentStatusText(): string {
    const transaction = this.paymentTransaction();
    if (!transaction) return '';
    return this.paymentService.getStatusText(transaction.status);
  }

  // Kitchen Print Methods
  canPrintKitchenTicket(): boolean {
    const order = this.order();
    if (!order) return false;
    // Can print/reprint for Submitted or InPreparation orders
    return order.status === OrderStatus.Submitted || order.status === OrderStatus.InPreparation;
  }

  printKitchenTicket(): void {
    const order = this.order();
    if (!order) return;

    this.kitchenPrintService.printKitchenTicket(order.id).subscribe({
      next: (result) => {
        this.toastService.success('Kitchen ticket printed');
        if (result.ticket) {
          this.kitchenPrintService.printTicketLocal(result.ticket);
        }
      },
      error: (error) => {
        console.error('Error printing kitchen ticket:', error);
        const errorMessage = error.error?.message || 'Failed to print kitchen ticket';
        this.toastService.error(errorMessage);
      }
    });
  }

  openKitchenReprintModal(): void {
    this.kitchenReprintReason.set('');
    this.showKitchenReprintModal.set(true);
  }

  closeKitchenReprintModal(): void {
    this.showKitchenReprintModal.set(false);
    this.kitchenReprintReason.set('');
  }

  confirmKitchenReprint(): void {
    const order = this.order();
    if (!order || !this.kitchenReprintReason().trim()) return;

    this.kitchenPrintService.reprintKitchenTicket(order.id, this.kitchenReprintReason().trim()).subscribe({
      next: (result) => {
        this.toastService.success('Kitchen ticket reprinted');
        this.closeKitchenReprintModal();
        if (result.ticket) {
          this.kitchenPrintService.printTicketLocal(result.ticket);
        }
      },
      error: (error) => {
        console.error('Error reprinting kitchen ticket:', error);
        const errorMessage = error.error?.message || 'Failed to reprint kitchen ticket';
        this.toastService.error(errorMessage);
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      month: '2-digit',
      day: '2-digit',
      year: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    }).format(date);
  }
}

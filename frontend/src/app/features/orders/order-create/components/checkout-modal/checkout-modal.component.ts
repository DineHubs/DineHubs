import { Component, Input, output, signal, OnInit, OnChanges, HostListener, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, X, CreditCard, Wallet, Smartphone } from 'lucide-angular';

export type PaymentProvider = 'Cash' | 'Card' | 'Mobile';

@Component({
  selector: 'app-checkout-modal',
  standalone: true,
  imports: [CommonModule, NgClass, FormsModule, LucideAngularModule],
  templateUrl: './checkout-modal.component.html',
  styleUrl: './checkout-modal.component.scss'
})
export class CheckoutModalComponent implements OnInit, OnChanges, AfterViewInit {
  @Input({ required: true }) isOpen = false;
  @Input({ required: true }) orderTotal!: number;
  @Input() isSubmitting = false;
  @ViewChild('modalContent') modalContent?: ElementRef<HTMLDivElement>;

  closed = output<void>();
  paymentProcessed = output<{ provider: PaymentProvider; amount: number }>();

  selectedProvider = signal<PaymentProvider>('Cash');
  amount = signal<number>(0);
  paymentProviders: PaymentProvider[] = ['Cash', 'Card', 'Mobile'];

  xIcon = X;
  creditCardIcon = CreditCard;
  walletIcon = Wallet;
  smartphoneIcon = Smartphone;

  ngOnInit(): void {
    this.amount.set(this.orderTotal);
  }

  ngOnChanges(): void {
    if (this.isOpen) {
      this.amount.set(this.orderTotal);
      // Focus management when modal opens
      setTimeout(() => {
        if (this.modalContent) {
          const firstInput = this.modalContent.nativeElement.querySelector('input, button') as HTMLElement;
          firstInput?.focus();
        }
      }, 100);
    }
  }

  ngAfterViewInit(): void {
    if (this.isOpen && this.modalContent) {
      const firstInput = this.modalContent.nativeElement.querySelector('input, button') as HTMLElement;
      firstInput?.focus();
    }
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent): void {
    if (this.isOpen && !this.isSubmitting) {
      this.close();
    }
  }

  close(): void {
    this.closed.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close();
    }
  }

  onProviderSelect(provider: PaymentProvider): void {
    this.selectedProvider.set(provider);
  }

  onAmountChange(value: string): void {
    const numValue = parseFloat(value) || 0;
    const maxAmount = this.orderTotal;
    this.amount.set(Math.min(Math.max(0, numValue), maxAmount));
  }

  processPayment(): void {
    if (this.amount() <= 0 || this.amount() > this.orderTotal) {
      return;
    }
    this.paymentProcessed.emit({
      provider: this.selectedProvider(),
      amount: this.amount()
    });
  }

  getProviderIcon(provider: PaymentProvider) {
    switch (provider) {
      case 'Cash':
        return this.walletIcon;
      case 'Card':
        return this.creditCardIcon;
      case 'Mobile':
        return this.smartphoneIcon;
      default:
        return this.creditCardIcon;
    }
  }

  getProviderLabel(provider: PaymentProvider): string {
    return provider;
  }
}


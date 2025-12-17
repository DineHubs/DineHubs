import { Component, Input, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ShoppingCart, Trash2, CreditCard } from 'lucide-angular';
import { CartItem } from '../../order-create.component';
import { CartItemComponent } from '../cart-item/cart-item.component';

@Component({
  selector: 'app-order-sidebar',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, CartItemComponent],
  templateUrl: './order-sidebar.component.html',
  styleUrl: './order-sidebar.component.scss'
})
export class OrderSidebarComponent {
  cart = input.required<CartItem[]>();
  @Input({ required: true }) cartTotal!: number;
  @Input({ required: true }) cartItemCount!: number;
  @Input({ required: true }) tableNumber!: string;
  @Input({ required: true }) isTakeAway!: boolean;
  @Input() isSubmitting = false;

  quantityChanged = output<{ item: CartItem; change: number }>();
  removed = output<CartItem>();
  tableNumberChanged = output<string>();
  takeAwayToggled = output<boolean>();
  clearCart = output<void>();
  checkout = output<void>();

  shoppingCartIcon = ShoppingCart;
  trashIcon = Trash2;
  creditCardIcon = CreditCard;

  hasItems = computed(() => this.cart().length > 0);

  onTableNumberChange(value: string): void {
    this.tableNumberChanged.emit(value);
  }

  onTakeAwayToggle(value: boolean): void {
    this.takeAwayToggled.emit(value);
  }
}


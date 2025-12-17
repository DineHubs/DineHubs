import { Component, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Plus, Minus, Trash2 } from 'lucide-angular';
import { CartItem } from '../../order-create.component';

@Component({
  selector: 'app-cart-item',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './cart-item.component.html',
  styleUrl: './cart-item.component.scss'
})
export class CartItemComponent {
  @Input({ required: true }) cartItem!: CartItem;
  
  quantityChanged = output<{ item: CartItem; change: number }>();
  removed = output<CartItem>();

  plusIcon = Plus;
  minusIcon = Minus;
  trashIcon = Trash2;

  onQuantityChange(change: number): void {
    this.quantityChanged.emit({ item: this.cartItem, change });
  }

  onRemove(): void {
    this.removed.emit(this.cartItem);
  }
}


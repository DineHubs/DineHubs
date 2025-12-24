import { Component, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuItem } from '../../../../../core/models/menu-item.model';
import { ProductCardComponent } from '../product-card/product-card.component';
import { CartItem } from '../../order-create.component';

@Component({
  selector: 'app-product-grid',
  standalone: true,
  imports: [CommonModule, ProductCardComponent],
  templateUrl: './product-grid.component.html',
  styleUrl: './product-grid.component.scss'
})
export class ProductGridComponent {
  @Input({ required: true }) menuItems: MenuItem[] = [];
  @Input() isLoading = false;
  @Input() cartItems: CartItem[] = [];
  
  addToCart = output<MenuItem>();
  quantityChanged = output<{ menuItemId: string; quantity: number }>();

  onAddToCart(menuItem: MenuItem): void {
    this.addToCart.emit(menuItem);
  }

  onQuantityChanged(event: { menuItemId: string; quantity: number }): void {
    this.quantityChanged.emit(event);
  }
}

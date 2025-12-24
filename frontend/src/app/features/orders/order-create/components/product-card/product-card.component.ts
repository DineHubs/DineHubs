import { Component, Input, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuItem } from '../../../../../core/models/menu-item.model';
import { LucideAngularModule, Plus, Minus } from 'lucide-angular';
import { CartItem } from '../../order-create.component';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss'
})
export class ProductCardComponent {
  @Input({ required: true }) menuItem!: MenuItem;
  cartItems = input<CartItem[]>([]);
  
  addToCart = output<MenuItem>();
  quantityChanged = output<{ menuItemId: string; quantity: number }>();
  
  plusIcon = Plus;
  minusIcon = Minus;

  quantity = computed(() => {
    const items = this.cartItems();
    const cartItem = items.find(item => item.menuItem.id === this.menuItem.id);
    return cartItem ? cartItem.quantity : 0;
  });

  getCategoryColor(category: string): string {
    const categoryLower = category.toLowerCase();
    const colors: Record<string, string> = {
      'food': 'bg-pink-500',
      'drink': 'bg-green-400',
      'drinks': 'bg-green-400',
      'beverage': 'bg-green-400',
      'dessert': 'bg-purple-400',
      'appetizer': 'bg-yellow-400',
      'combo': 'bg-indigo-400'
    };
    
    // Check for partial matches
    for (const [key, color] of Object.entries(colors)) {
      if (categoryLower.includes(key)) {
        return color;
      }
    }
    
    // Default colors based on common categories
    if (categoryLower.includes('burger') || categoryLower.includes('pizza') || categoryLower.includes('sushi') || categoryLower.includes('pasta')) {
      return 'bg-pink-500';
    }
    if (categoryLower.includes('cola') || categoryLower.includes('water') || categoryLower.includes('schweppes') || categoryLower.includes('fanta')) {
      return 'bg-green-400';
    }
    
    return 'bg-gray-400';
  }

  onAddToCart(): void {
    this.addToCart.emit(this.menuItem);
  }

  onIncrement(event: Event): void {
    event.stopPropagation();
    this.addToCart.emit(this.menuItem);
  }

  onDecrement(event: Event): void {
    event.stopPropagation();
    const currentQty = this.quantity();
    if (currentQty > 0) {
      this.quantityChanged.emit({ 
        menuItemId: this.menuItem.id, 
        quantity: currentQty - 1 
      });
    }
  }
}

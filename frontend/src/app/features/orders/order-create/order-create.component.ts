import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { MenuItem } from '../../../core/models/menu-item.model';
import { CreateOrderRequest, OrderLine } from '../../../core/models/order.model';

interface CartItem {
  menuItem: MenuItem;
  quantity: number;
}

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatChipsModule,
    MatBadgeModule,
    MatDividerModule
  ],
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.scss'
})
export class OrderCreateComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  menuItems: MenuItem[] = [];
  filteredItems: MenuItem[] = [];
  cart: CartItem[] = [];
  tableNumber: string = '';
  isTakeAway: boolean = false;
  isLoading = false;
  isSubmitting = false;
  selectedCategory = 'All';
  categories: string[] = [];
  searchTerm = '';

  ngOnInit(): void {
    // Access control handled by route guard
    this.loadMenuItems();
  }

  loadMenuItems(): void {
    this.isLoading = true;
    this.apiService.get<MenuItem[]>('MenuItems').subscribe({
      next: (items) => {
        this.menuItems = items.filter(item => item.isAvailable);
        this.filteredItems = this.menuItems;
        this.categories = ['All', ...new Set(this.menuItems.map(item => item.category))];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading menu items:', error);
        this.snackBar.open('Failed to load menu items', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  filterByCategory(category: string): void {
    this.selectedCategory = category;
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = this.menuItems;

    if (this.selectedCategory !== 'All') {
      filtered = filtered.filter(item => item.category === this.selectedCategory);
    }

    if (this.searchTerm.trim()) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(search) ||
        item.category.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  addToCart(menuItem: MenuItem): void {
    const existingItem = this.cart.find(item => item.menuItem.id === menuItem.id);
    if (existingItem) {
      existingItem.quantity++;
    } else {
      this.cart.push({ menuItem, quantity: 1 });
    }
    this.snackBar.open(`${menuItem.name} added to cart`, 'Close', { duration: 2000 });
  }

  removeFromCart(cartItem: CartItem): void {
    const index = this.cart.indexOf(cartItem);
    if (index > -1) {
      this.cart.splice(index, 1);
    }
  }

  updateQuantity(cartItem: CartItem, change: number): void {
    cartItem.quantity += change;
    if (cartItem.quantity <= 0) {
      this.removeFromCart(cartItem);
    }
  }

  getCartTotal(): number {
    return this.cart.reduce((total, item) => total + (item.menuItem.price * item.quantity), 0);
  }

  getCartItemCount(): number {
    return this.cart.reduce((count, item) => count + item.quantity, 0);
  }

  submitOrder(): void {
    if (this.cart.length === 0) {
      this.snackBar.open('Please add items to cart before submitting', 'Close', { duration: 3000 });
      return;
    }

    if (!this.isTakeAway && !this.tableNumber.trim()) {
      this.snackBar.open('Please enter a table number or select Take Away', 'Close', { duration: 3000 });
      return;
    }

    this.isSubmitting = true;

    try {
      // Validate and convert menu item IDs to ensure they're valid GUIDs
      const orderLines: OrderLine[] = this.cart.map(item => {
        // Ensure menuItemId is a valid GUID string
        const menuItemId = item.menuItem.id;
        if (!menuItemId || !this.isValidGuid(menuItemId)) {
          throw new Error(`Invalid menu item ID: ${menuItemId}`);
        }
        
        return {
          menuItemId: menuItemId,
          name: item.menuItem.name,
          price: item.menuItem.price,
          quantity: item.quantity
        };
      });

      const orderRequest: CreateOrderRequest = {
        isTakeAway: this.isTakeAway,
        tableNumber: this.isTakeAway ? undefined : this.tableNumber.trim(),
        items: orderLines
      };

      this.apiService.post<any>('Orders', orderRequest).subscribe({
        next: (response) => {
          this.snackBar.open('Order created successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/orders']);
        },
        error: (error) => {
          console.error('Error creating order:', error);
          // Extract error message from various possible locations
          let errorMessage = 'Failed to create order';
          if (error.error) {
            if (error.error.message) {
              errorMessage = error.error.message;
            } else if (error.error.errors && typeof error.error.errors === 'object') {
              // FluentValidation/ModelState errors - can be array or object
              const errorValues = Object.values(error.error.errors);
              const validationErrors: string[] = [];
              errorValues.forEach(err => {
                if (Array.isArray(err)) {
                  validationErrors.push(...err);
                } else if (typeof err === 'string') {
                  validationErrors.push(err);
                } else if (err && typeof err === 'object' && 'message' in err) {
                  validationErrors.push((err as any).message);
                }
              });
              errorMessage = validationErrors.length > 0 
                ? validationErrors.join(', ') 
                : errorMessage;
            } else if (error.error.details) {
              errorMessage = error.error.details;
            } else if (typeof error.error === 'string') {
              errorMessage = error.error;
            }
          }
          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          this.isSubmitting = false;
        }
      });
    } catch (error: any) {
      console.error('Error preparing order:', error);
      this.snackBar.open(error.message || 'Failed to prepare order', 'Close', { duration: 5000 });
      this.isSubmitting = false;
    }
  }

  clearCart(): void {
    this.cart = [];
  }

  private isValidGuid(value: string): boolean {
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    return guidRegex.test(value);
  }
}


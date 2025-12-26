import { Component, OnInit, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { ToastService } from '../../../core/services/toast.service';
import { PrintService } from '../../../core/services/print.service';
import { MenuItem } from '../../../core/models/menu-item.model';
import { CreateOrderRequest, OrderLine } from '../../../core/models/order.model';
import { ProductGridComponent } from './components/product-grid/product-grid.component';
import { CategoryNavComponent } from './components/category-nav/category-nav.component';
import { OrderSidebarComponent } from './components/order-sidebar/order-sidebar.component';
import { CheckoutModalComponent, PaymentProvider } from './components/checkout-modal/checkout-modal.component';
import { LucideAngularModule, Search, Moon, Sun, ShoppingCart, X } from 'lucide-angular';

export interface CartItem {
  menuItem: MenuItem;
  quantity: number;
}

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ProductGridComponent,
    CategoryNavComponent,
    OrderSidebarComponent,
    CheckoutModalComponent,
    LucideAngularModule
  ],
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.scss'
})
export class OrderCreateComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private themeService = inject(ThemeService);
  private toastService = inject(ToastService);
  private printService = inject(PrintService);

  // Table info from floor plan navigation
  tableId = signal<string | null>(null);

  // Signals for state management
  menuItems = signal<MenuItem[]>([]);
  cart = signal<CartItem[]>([]);
  tableNumber = signal<string>('');
  isTakeAway = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  selectedCategory = signal<string>('All');
  searchTerm = signal<string>('');

  // Computed values
  categories = computed(() => {
    const items = this.menuItems();
    return ['All', ...new Set(items.map(item => item.category))];
  });

  filteredItems = computed(() => {
    let filtered = this.menuItems();
    const category = this.selectedCategory();
    const search = this.searchTerm().trim().toLowerCase();

    if (category !== 'All') {
      filtered = filtered.filter(item => item.category === category);
    }

    if (search) {
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(search) ||
        item.category.toLowerCase().includes(search)
      );
    }

    return filtered;
  });

  cartTotal = computed(() => {
    return this.cart().reduce((total, item) =>
      total + (item.menuItem.price * item.quantity), 0
    );
  });

  cartItemCount = computed(() => {
    return this.cart().reduce((count, item) => count + item.quantity, 0);
  });

  // Checkout modal state
  isCheckoutOpen = signal<boolean>(false);

  // Mobile cart drawer state
  isCartDrawerOpen = signal<boolean>(false);
  isMobileView = signal<boolean>(false);

  // Icons
  searchIcon = Search;
  moonIcon = Moon;
  sunIcon = Sun;
  cartIcon = ShoppingCart;
  closeIcon = X;

  @HostListener('window:resize')
  onResize(): void {
    this.checkMobileView();
  }

  private checkMobileView(): void {
    this.isMobileView.set(window.innerWidth < 1024);
  }

  get themeIcon() {
    return this.themeService.theme() === 'dark' ? this.sunIcon : this.moonIcon;
  }

  get isDarkMode() {
    return this.themeService.theme() === 'dark';
  }

  ngOnInit(): void {
    this.checkMobileView();
    this.loadMenuItems();
    
    // Check for table info from floor plan navigation
    this.route.queryParams.subscribe(params => {
      if (params['tableId']) {
        this.tableId.set(params['tableId']);
      }
      if (params['tableNumber']) {
        this.tableNumber.set(params['tableNumber']);
      }
    });
  }

  toggleCartDrawer(): void {
    this.isCartDrawerOpen.update(v => !v);
  }

  closeCartDrawer(): void {
    this.isCartDrawerOpen.set(false);
  }

  loadMenuItems(): void {
    this.isLoading.set(true);
    this.apiService.get<MenuItem[]>('MenuItems').subscribe({
      next: (items) => {
        this.menuItems.set(items.filter(item => item.isAvailable));
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading menu items:', error);
        this.isLoading.set(false);
        this.toastService.error('Failed to load menu items');
      }
    });
  }

  filterByCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
  }

  addToCart(menuItem: MenuItem): void {
    const currentCart = this.cart();
    const existingItem = currentCart.find(item => item.menuItem.id === menuItem.id);

    if (existingItem) {
      this.cart.set(
        currentCart.map(item =>
          item.menuItem.id === menuItem.id
            ? { ...item, quantity: item.quantity + 1 }
            : item
        )
      );
    } else {
      this.cart.set([...currentCart, { menuItem, quantity: 1 }]);
    }
    this.toastService.success(`${menuItem.name} added to cart`);
  }

  removeFromCart(cartItem: CartItem): void {
    this.cart.set(
      this.cart().filter(item => item !== cartItem)
    );
  }

  updateQuantity(cartItem: CartItem, change: number): void {
    const newQuantity = cartItem.quantity + change;
    if (newQuantity <= 0) {
      this.removeFromCart(cartItem);
    } else {
      this.cart.set(
        this.cart().map(item =>
          item === cartItem
            ? { ...item, quantity: newQuantity }
            : item
        )
      );
    }
  }

  clearCart(): void {
    this.cart.set([]);
  }

  onCheckout(): void {
    const currentCart = this.cart();

    if (currentCart.length === 0) {
      this.toastService.error('Please add items to cart before checkout');
      return;
    }

    const isTakeAway = this.isTakeAway();
    const tableNum = this.tableNumber().trim();

    if (!isTakeAway && !tableNum) {
      this.toastService.error('Please enter a table number or select Take Away');
      return;
    }

    this.isCheckoutOpen.set(true);
  }

  onPaymentProcessed(payment: { provider: PaymentProvider; amount: number }): void {
    this.submitOrder();
  }

  onCheckoutClosed(): void {
    this.isCheckoutOpen.set(false);
  }

  submitOrder(): void {
    const currentCart = this.cart();

    if (currentCart.length === 0) {
      this.toastService.error('Please add items to cart before submitting');
      return;
    }

    const isTakeAway = this.isTakeAway();
    const tableNum = this.tableNumber().trim();

    if (!isTakeAway && !tableNum) {
      this.toastService.error('Please enter a table number or select Take Away');
      return;
    }

    this.isSubmitting.set(true);

    try {
      const orderLines: OrderLine[] = currentCart.map(item => {
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
        isTakeAway: isTakeAway,
        tableNumber: isTakeAway ? undefined : tableNum,
        items: orderLines
      };

      this.apiService.post<any>('Orders', orderRequest).subscribe({
        next: (response) => {
          this.toastService.success('Order created successfully!');
          this.isCheckoutOpen.set(false);
          
          // Set up navigation handler before printing
          if (typeof window !== 'undefined') {
            const originalAfterPrint = window.onafterprint;
            window.onafterprint = (ev: Event) => {
              // Call original handler first (to clear print data in PrintService)
              if (originalAfterPrint) {
                originalAfterPrint.call(window, ev);
              }
              // Then navigate
              this.router.navigate(['/orders']);
            };
            
            // Fallback: navigate after 3 seconds if print dialog doesn't trigger onafterprint
            // (user might have blocked print dialog or it might not fire in some browsers)
            setTimeout(() => {
              if (window.onafterprint) {
                const handler = window.onafterprint;
                // Clear handler to prevent double navigation
                window.onafterprint = null;
                // Create a mock event for the handler
                const mockEvent = new Event('afterprint');
                handler.call(window, mockEvent);
              }
            }, 3000);
          }
          
          // Trigger print
          this.printService.printOrder(response);
        },
        error: (error) => {
          console.error('Error creating order:', error);
          let errorMessage = 'Failed to create order';
          if (error.error) {
            if (error.error.message) {
              errorMessage = error.error.message;
            } else if (error.error.errors && typeof error.error.errors === 'object') {
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
          this.toastService.error(errorMessage);
          this.isSubmitting.set(false);
        }
      });
    } catch (error: any) {
      console.error('Error preparing order:', error);
      this.toastService.error(error.message || 'Failed to prepare order');
      this.isSubmitting.set(false);
    }
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onCartQuantityChange(event: { item: CartItem; change: number }): void {
    this.updateQuantity(event.item, event.change);
  }

  onCartItemRemoved(item: CartItem): void {
    this.removeFromCart(item);
  }

  onGridQuantityChange(event: { menuItemId: string; quantity: number }): void {
    const currentCart = this.cart();
    if (event.quantity <= 0) {
      // Remove item from cart
      this.cart.set(currentCart.filter(item => item.menuItem.id !== event.menuItemId));
    } else {
      // Update quantity
      this.cart.set(
        currentCart.map(item =>
          item.menuItem.id === event.menuItemId
            ? { ...item, quantity: event.quantity }
            : item
        )
      );
    }
  }

  onTableNumberChange(value: string): void {
    this.tableNumber.set(value);
  }

  onTakeAwayToggle(value: boolean): void {
    this.isTakeAway.set(value);
  }

  private isValidGuid(value: string): boolean {
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    return guidRegex.test(value);
  }
}


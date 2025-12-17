import { Component, OnInit, OnDestroy, inject, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { 
  LucideAngularModule, 
  ChevronRight, 
  ChevronDown,
  LayoutDashboard,
  ShoppingCart,
  Utensils,
  Users,
  Settings,
  Building2,
  Store,
  ChefHat,
  Table,
  CreditCard,
  BarChart3,
  Receipt,
  QrCode,
  FileText,
  Package,
  Circle
} from 'lucide-angular';
import { NavigationService } from '../../core/services/navigation.service';
import { NavigationMenuItem } from '../../core/models/navigation.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LucideAngularModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit, OnDestroy {
  private navigationService = inject(NavigationService);
  private authService = inject(AuthService);
  private router = inject(Router);
  
  menuItems = this.navigationService.menuItems;
  expandedItems = signal<Set<string>>(new Set());
  
  chevronRightIcon = ChevronRight;
  chevronDownIcon = ChevronDown;

  constructor() {
    // Use effect to react to authentication state changes
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      
      if (isAuthenticated) {
        // Load menu when user is authenticated
        this.loadMenu();
      } else {
        // Clear menu when logged out
        this.navigationService.menuItems.set([]);
      }
    });
  }

  ngOnInit(): void {
    // Load menu on initialization if already authenticated
    if (this.authService.isAuthenticated()) {
      this.loadMenu();
    }
  }

  ngOnDestroy(): void {
    // Effect cleanup is handled automatically by Angular
  }

  private loadMenu(): void {
    this.navigationService.loadMenu().subscribe();
  }

  hasChildren(item: NavigationMenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  isExpanded(itemId: string): boolean {
    return this.expandedItems().has(itemId);
  }

  toggleExpanded(itemId: string): void {
    const current = new Set(this.expandedItems());
    if (current.has(itemId)) {
      current.delete(itemId);
    } else {
      current.add(itemId);
    }
    this.expandedItems.set(current);
  }

  getIcon(iconName?: string): any {
    if (!iconName) return Circle;
    
    // Map Material icon names to Lucide icons
    const iconMap: { [key: string]: any } = {
      'dashboard': LayoutDashboard,
      'shopping_cart': ShoppingCart,
      'restaurant_menu': Utensils,
      'people': Users,
      'settings': Settings,
      'business': Building2,
      'store': Store,
      'kitchen': ChefHat,
      'table_restaurant': Table,
      'subscriptions': CreditCard,
      'bar_chart': BarChart3,
      'receipt': Receipt,
      'qr_code': QrCode,
      'report': FileText,
      'inventory': Package
    };

    return iconMap[iconName] || Circle;
  }

  hasIcon(iconName?: string): boolean {
    return !!iconName;
  }

  isActive(route?: string): boolean {
    if (!route) return false;
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }

  navigateTo(route?: string): void {
    if (route) {
      this.router.navigate([route]);
    }
  }
}

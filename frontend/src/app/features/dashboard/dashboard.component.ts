import { Component, OnInit, inject, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LucideAngularModule, RefreshCw, AlertCircle, LayoutDashboard, ShoppingCart, Utensils, Users, Building2, ChefHat, Table, CreditCard, TrendingUp, DollarSign, Package, Circle } from 'lucide-angular';
import { ToastService } from '../../core/services/toast.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { BranchContextService, Branch } from '../../core/services/branch-context.service';
import { AppRoles } from '../../core/constants/roles.constants';
import {
  DashboardStats,
  SalesTrend,
  TopSellingItem,
  OrderStatusCount,
  OrderHourlyCount,
  SubscriptionStatusCount,
  SubscriptionTrend
} from '../../core/models/dashboard.model';
import { SalesTrendChartComponent } from '../../shared/components/sales-trend-chart/sales-trend-chart.component';
import { PieChartComponent } from '../../shared/components/pie-chart/pie-chart.component';
import { BarChartComponent } from '../../shared/components/bar-chart/bar-chart.component';
import { Subject, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LucideAngularModule,
    SalesTrendChartComponent,
    PieChartComponent,
    BarChartComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private branchContextService = inject(BranchContextService);
  private destroy$ = new Subject<void>();

  // Icons
  refreshIcon = RefreshCw;
  alertCircleIcon = AlertCircle;
  dashboardIcon = LayoutDashboard;
  shoppingCartIcon = ShoppingCart;
  utensilsIcon = Utensils;
  usersIcon = Users;
  buildingIcon = Building2;
  chefHatIcon = ChefHat;
  tableIcon = Table;
  creditCardIcon = CreditCard;
  trendingUpIcon = TrendingUp;
  dollarSignIcon = DollarSign;
  packageIcon = Package;

  // Data
  stats: DashboardStats | null = null;
  salesTrend: SalesTrend[] = [];
  topItems: TopSellingItem[] = [];
  ordersByStatus: OrderStatusCount[] = [];
  ordersByHour: OrderHourlyCount[] = [];

  // UI State
  isLoading = signal(true);
  hasError = signal(false);
  errorMessage = signal('');
  selectedPeriod = signal<'today' | 'week' | 'month' | 'year'>('today');
  
  // Branch selection (Admin only)
  branches = signal<Branch[]>([]);
  selectedBranchId = signal<string | null>(null);
  branchesLoading = signal(false);
  
  // Stats cards
  statCards: Array<{
    title: string;
    value: string;
    icon: string;
    color: string;
    route?: string;
  }> = [];

  // Chart data
  statusChartData: { label: string; value: number }[] = [];
  topItemsChartData: { label: string; value: number }[] = [];
  hourlyChartData: { label: string; value: number }[] = [];
  subscriptionStatusData: { label: string; value: number }[] = [];
  subscriptionTrendData: { label: string; value: number }[] = [];

  // User roles
  isManager = false;
  isAdmin = false;
  isSuperAdmin = false;
  isKitchen = false;
  isWaiter = false;
  isInventoryManager = false;

  // Check if dashboard is empty (no data)
  get isEmpty(): boolean {
    if (!this.stats) return false;
    return this.stats.thisMonthRevenue === 0 && 
           this.stats.thisMonthOrdersCount === 0 && 
           this.stats.todayRevenue === 0 && 
           this.stats.todayOrdersCount === 0;
  }

  ngOnInit(): void {
    this.checkUserRoles();
    
    // Load branches for Admin or Manager role only
    if (this.isAdmin || this.isManager) {
      this.loadBranches();
    } else {
      // Clear any stored branch selection for non-admin/manager users
      this.branchContextService.clearSelection();
      this.branches.set([]);
      this.selectedBranchId.set(null);
    }
    
    this.loadDashboardData();
    
    // Auto-refresh every 60 seconds
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadDashboardData();
      });
  }

  private loadBranches(): void {
    this.branchesLoading.set(true);
    this.branchContextService.loadBranches().subscribe({
      next: (branches) => {
        this.branches.set(branches);
        this.branchesLoading.set(false);
        
        // Restore selected branch from context
        const selectedBranch = this.branchContextService.selectedBranch();
        if (selectedBranch) {
          this.selectedBranchId.set(selectedBranch.id);
        }
      },
      error: (error) => {
        console.error('Error loading branches:', error);
        this.branchesLoading.set(false);
        this.branches.set([]);
      }
    });
  }

  onBranchChange(branchId: string): void {
    const newBranchId = branchId === '' ? null : branchId;
    this.selectedBranchId.set(newBranchId);
    this.branchContextService.selectBranchById(newBranchId);
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private checkUserRoles(): void {
    this.isManager = this.authService.hasRole(AppRoles.Manager);
    this.isAdmin = this.authService.hasRole(AppRoles.Admin);
    this.isSuperAdmin = this.authService.hasRole(AppRoles.SuperAdmin);
    this.isKitchen = this.authService.hasRole(AppRoles.Kitchen);
    this.isWaiter = this.authService.hasRole(AppRoles.Waiter);
    this.isInventoryManager = this.authService.hasRole(AppRoles.InventoryManager);
  }

  private getDateRange(period: string): { from: Date; to: Date } {
    const now = new Date();
    const to = new Date(now);
    let from = new Date(now);

    switch (period) {
      case 'today':
        from.setHours(0, 0, 0, 0);
        to.setHours(23, 59, 59, 999);
        break;
      case 'week':
        from.setDate(now.getDate() - 7);
        from.setHours(0, 0, 0, 0);
        break;
      case 'month':
        from.setDate(1);
        from.setHours(0, 0, 0, 0);
        break;
      case 'year':
        from.setMonth(0, 1);
        from.setHours(0, 0, 0, 0);
        break;
    }

    return { from, to };
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.errorMessage.set('');
    const { from, to } = this.getDateRange(this.selectedPeriod());
    const branchId = this.selectedBranchId();

    // Load stats
    this.dashboardService.getStats(from, to, branchId).subscribe({
      next: (stats) => {
        try {
          this.stats = stats;
          this.updateStatCards();
          this.isLoading.set(false);
          this.hasError.set(false);
        } catch (error: any) {
          console.error('Error processing dashboard stats:', error);
          this.handleError('Failed to process dashboard statistics');
        }
      },
      error: (error) => {
        console.error('Error loading dashboard stats:', error);
        this.handleError('Failed to load dashboard statistics. Please try again later.');
      }
    });

    // Load charts based on roles
    if (this.isManager || this.isAdmin) {
      this.loadManagerCharts(from, to, branchId);
    }

    if (this.isSuperAdmin) {
      this.loadSuperAdminCharts();
    }

    if (this.isKitchen) {
      this.loadKitchenCharts(branchId);
    }
  }

  private handleError(message: string): void {
    this.hasError.set(true);
    this.errorMessage.set(message);
    this.isLoading.set(false);
    this.toastService.error(message);
  }

  private loadManagerCharts(from: Date, to: Date, branchId: string | null): void {
    // Sales trend
    this.dashboardService.getSalesTrend(from, to, branchId).subscribe({
      next: (data) => {
        try {
          this.salesTrend = data || [];
        } catch (error: any) {
          console.error('Error processing sales trend data:', error);
          this.salesTrend = [];
        }
      },
      error: (error) => {
        console.error('Error loading sales trend:', error);
        this.salesTrend = [];
        this.toastService.error('Failed to load sales trend chart');
      }
    });

    // Top selling items
    this.dashboardService.getTopItems(10, from, to, branchId).subscribe({
      next: (items) => {
        try {
          this.topItems = items || [];
          this.topItemsChartData = this.topItems.map(item => ({
            label: item.name,
            value: item.totalQuantity
          }));
        } catch (error: any) {
          console.error('Error processing top items data:', error);
          this.topItems = [];
          this.topItemsChartData = [];
        }
      },
      error: (error) => {
        console.error('Error loading top items:', error);
        this.topItems = [];
        this.topItemsChartData = [];
        this.toastService.error('Failed to load top selling items chart');
      }
    });

    // Orders by status
    this.dashboardService.getOrdersByStatus(from, to, branchId).subscribe({
      next: (data) => {
        try {
          this.ordersByStatus = data || [];
          this.statusChartData = this.ordersByStatus.map(d => ({
            label: d.status,
            value: d.count
          }));
        } catch (error: any) {
          console.error('Error processing orders by status data:', error);
          this.ordersByStatus = [];
          this.statusChartData = [];
        }
      },
      error: (error) => {
        console.error('Error loading orders by status:', error);
        this.ordersByStatus = [];
        this.statusChartData = [];
        this.toastService.error('Failed to load orders by status chart');
      }
    });

    // Orders by hour (today)
    this.dashboardService.getOrdersByHour(new Date(), branchId).subscribe({
      next: (data) => {
        try {
          this.ordersByHour = data || [];
          this.hourlyChartData = this.ordersByHour.map(d => ({
            label: `${d.hour}:00`,
            value: d.orderCount
          }));
        } catch (error: any) {
          console.error('Error processing hourly orders data:', error);
          this.ordersByHour = [];
          this.hourlyChartData = [];
        }
      },
      error: (error) => {
        console.error('Error loading hourly orders:', error);
        this.ordersByHour = [];
        this.hourlyChartData = [];
        this.toastService.error('Failed to load hourly orders chart');
      }
    });
  }

  private loadKitchenCharts(branchId: string | null): void {
    const { from, to } = this.getDateRange('today');
    this.dashboardService.getOrdersByStatus(from, to, branchId).subscribe({
      next: (data) => {
        try {
          this.ordersByStatus = (data || []).filter(d => 
            ['Submitted', 'InPreparation', 'Ready'].includes(d.status)
          );
          this.statusChartData = this.ordersByStatus.map(d => ({
            label: d.status,
            value: d.count
          }));
        } catch (error: any) {
          console.error('Error processing kitchen orders data:', error);
          this.ordersByStatus = [];
          this.statusChartData = [];
        }
      },
      error: (error) => {
        console.error('Error loading kitchen orders:', error);
        this.ordersByStatus = [];
        this.statusChartData = [];
        this.toastService.error('Failed to load kitchen order statistics');
      }
    });
  }

  private loadSuperAdminCharts(): void {
    // Subscription status breakdown
    this.dashboardService.getSubscriptionStatusBreakdown().subscribe({
      next: (data) => {
        try {
          this.subscriptionStatusData = (data || []).map(d => ({
            label: d.status,
            value: d.count
          }));
        } catch (error: any) {
          console.error('Error processing subscription status data:', error);
          this.subscriptionStatusData = [];
        }
      },
      error: (error) => {
        console.error('Error loading subscription status breakdown:', error);
        this.subscriptionStatusData = [];
        this.toastService.error('Failed to load subscription status chart');
      }
    });

    // Subscription trend
    this.dashboardService.getSubscriptionTrend(6).subscribe({
      next: (data) => {
        try {
          this.subscriptionTrendData = (data || []).map(d => ({
            label: d.month,
            value: d.count
          }));
        } catch (error: any) {
          console.error('Error processing subscription trend data:', error);
          this.subscriptionTrendData = [];
        }
      },
      error: (error) => {
        console.error('Error loading subscription trend:', error);
        this.subscriptionTrendData = [];
        this.toastService.error('Failed to load subscription trend chart');
      }
    });
  }

  private updateStatCards(): void {
    try {
      if (!this.stats) return;

      this.statCards = [];

      // Waiter sees their own stats instead of overall branch stats
      if (this.isWaiter && !this.isManager && !this.isAdmin) {
        this.statCards.push(
          {
            title: 'My Orders Today',
            value: this.stats.myOrdersTodayCount.toString(),
            icon: 'receipt',
            color: '#1976d2',
            route: '/orders'
          },
          {
            title: 'My Revenue Today',
            value: `RM ${this.stats.myOrdersTodayRevenue.toFixed(2)}`,
            icon: 'attach_money',
            color: '#388e3c'
          },
          {
            title: 'Active Tables',
            value: this.stats.activeTablesCount.toString(),
            icon: 'table_restaurant',
            color: '#f57c00',
            route: '/tables'
          },
          {
            title: 'Pending Orders',
            value: this.stats.pendingOrdersCount.toString(),
            icon: 'pending_actions',
            color: '#d32f2f',
            route: '/kitchen'
          }
        );
      } else {
        // Non-waiter roles see overall stats
        this.statCards.push(
      {
        title: 'Today\'s Orders',
        value: this.stats.todayOrdersCount.toString(),
        icon: 'receipt',
        color: '#1976d2',
        route: '/orders'
      },
      {
        title: 'Today\'s Revenue',
        value: `RM ${this.stats.todayRevenue.toFixed(2)}`,
        icon: 'attach_money',
        color: '#388e3c',
        route: '/reports'
      },
      {
        title: 'Active Tables',
        value: this.stats.activeTablesCount.toString(),
        icon: 'table_restaurant',
        color: '#f57c00',
        route: '/tables'
      },
      {
        title: 'Pending Orders',
        value: this.stats.pendingOrdersCount.toString(),
        icon: 'pending_actions',
        color: '#d32f2f',
        route: '/kitchen'
      }
        );
      }

    // Add role-specific stats
    if (this.isManager || this.isAdmin || this.isSuperAdmin) {
      this.statCards.push(
        {
          title: 'This Month Revenue',
          value: `RM ${this.stats.thisMonthRevenue.toFixed(2)}`,
          icon: 'trending_up',
          color: '#7b1fa2'
        },
        {
          title: 'This Month Orders',
          value: this.stats.thisMonthOrdersCount.toString(),
          icon: 'shopping_cart',
          color: '#0288d1'
        }
      );
    }

    if (this.isAdmin || this.isSuperAdmin) {
      this.statCards.push(
        {
          title: 'Active Branches',
          value: this.stats.activeBranchesCount.toString(),
          icon: 'store',
          color: '#455a64'
        },
        {
          title: 'Active Users',
          value: this.stats.activeUsersCount.toString(),
          icon: 'people',
          color: '#00796b'
        }
      );
    }

    if (this.isSuperAdmin) {
      this.statCards.push(
        {
          title: 'Active Tenants',
          value: this.stats.activeTenantsCount.toString(),
          icon: 'business',
          color: '#388e3c'
        },
        {
          title: 'New Subscriptions',
          value: this.stats.newSubscriptionsThisMonth.toString(),
          icon: 'add_circle',
          color: '#1976d2'
        },
        {
          title: 'Active Subscriptions',
          value: this.stats.totalActiveSubscriptions.toString(),
          icon: 'verified',
          color: '#7b1fa2'
        }
      );
    }

    if (this.isKitchen) {
      this.statCards.push({
        title: 'Orders in Queue',
        value: this.stats.ordersInQueueCount.toString(),
        icon: 'queue',
        color: '#d32f2f'
      });
    }

    if (this.isInventoryManager) {
      this.statCards.push(
        {
          title: 'Low Stock Items',
          value: this.stats.lowStockItemsCount.toString(),
          icon: 'warning',
          color: '#f57c00'
        }
      );
    }
  } catch (error: any) {
    console.error('Error updating stat cards:', error);
    this.statCards = [];
    this.toastService.error('Failed to display statistics cards');
  }
}

  onPeriodChange(period: 'today' | 'week' | 'month' | 'year'): void {
    this.selectedPeriod.set(period);
    this.loadDashboardData();
  }

  formatCurrency(value: number): string {
    return `RM ${value.toFixed(2)}`;
  }

  getIconForStat(iconName: string): any {
    const iconMap: { [key: string]: any } = {
      'trending_up': TrendingUp,
      'shopping_cart': ShoppingCart,
      'dollar': DollarSign,
      'store': Building2,
      'people': Users,
      'business': Building2,
      'add_circle': CreditCard,
      'verified': CreditCard,
      'queue': Package,
      'warning': AlertCircle
    };
    return iconMap[iconName] || Circle;
  }
}

import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
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
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatSnackBarModule,
    RouterModule,
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
  private snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<void>();

  // Data
  stats: DashboardStats | null = null;
  salesTrend: SalesTrend[] = [];
  topItems: TopSellingItem[] = [];
  ordersByStatus: OrderStatusCount[] = [];
  ordersByHour: OrderHourlyCount[] = [];

  // UI State
  isLoading = true;
  hasError = false;
  errorMessage = '';
  selectedPeriod: 'today' | 'week' | 'month' | 'year' = 'today';
  
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
    this.loadDashboardData();
    
    // Auto-refresh every 60 seconds
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadDashboardData();
      });
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
    this.isLoading = true;
    this.hasError = false;
    this.errorMessage = '';
    const { from, to } = this.getDateRange(this.selectedPeriod);

    // Load stats
    this.dashboardService.getStats(from, to).subscribe({
      next: (stats) => {
        try {
          this.stats = stats;
          this.updateStatCards();
          this.isLoading = false;
          this.hasError = false;
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
      this.loadManagerCharts(from, to);
    }

    if (this.isSuperAdmin) {
      this.loadSuperAdminCharts();
    }

    if (this.isKitchen) {
      this.loadKitchenCharts();
    }
  }

  private handleError(message: string): void {
    this.hasError = true;
    this.errorMessage = message;
    this.isLoading = false;
    this.snackBar.open(message, 'Close', { 
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  private loadManagerCharts(from: Date, to: Date): void {
    // Sales trend
    this.dashboardService.getSalesTrend(from, to).subscribe({
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
        this.snackBar.open('Failed to load sales trend chart', 'Close', { duration: 3000 });
      }
    });

    // Top selling items
    this.dashboardService.getTopItems(10, from, to).subscribe({
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
        this.snackBar.open('Failed to load top selling items chart', 'Close', { duration: 3000 });
      }
    });

    // Orders by status
    this.dashboardService.getOrdersByStatus(from, to).subscribe({
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
        this.snackBar.open('Failed to load orders by status chart', 'Close', { duration: 3000 });
      }
    });

    // Orders by hour (today)
    this.dashboardService.getOrdersByHour(new Date()).subscribe({
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
        this.snackBar.open('Failed to load hourly orders chart', 'Close', { duration: 3000 });
      }
    });
  }

  private loadKitchenCharts(): void {
    const { from, to } = this.getDateRange('today');
    this.dashboardService.getOrdersByStatus(from, to).subscribe({
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
        this.snackBar.open('Failed to load kitchen order statistics', 'Close', { duration: 3000 });
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
        this.snackBar.open('Failed to load subscription status chart', 'Close', { duration: 3000 });
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
        this.snackBar.open('Failed to load subscription trend chart', 'Close', { duration: 3000 });
      }
    });
  }

  private updateStatCards(): void {
    try {
      if (!this.stats) return;

      this.statCards = [
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
    ];

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
    this.snackBar.open('Failed to display statistics cards', 'Close', { duration: 3000 });
  }
}

  onPeriodChange(period: 'today' | 'week' | 'month' | 'year'): void {
    this.selectedPeriod = period;
    this.loadDashboardData();
  }

  formatCurrency(value: number): string {
    return `RM ${value.toFixed(2)}`;
  }
}

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { LucideAngularModule, RefreshCw, Plus, Eye, ShoppingCart, Clock, CheckCircle2, ChefHat, Truck, XCircle, CreditCard, FileText, Search, Filter, ChevronLeft, ChevronRight, X } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LucideAngularModule
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  allOrders = signal<Order[]>([]);
  isLoading = signal<boolean>(false);

  // Filter signals
  searchTerm = signal<string>('');
  statusFilter = signal<number | null>(null); // null = all statuses
  tableFilter = signal<'all' | 'table' | 'takeaway'>('all');

  // Pagination signals
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [10, 25, 50, 100];

  // Icons
  refreshIcon = RefreshCw;
  plusIcon = Plus;
  eyeIcon = Eye;
  shoppingCartIcon = ShoppingCart;
  clockIcon = Clock;
  checkCircleIcon = CheckCircle2;
  chefHatIcon = ChefHat;
  truckIcon = Truck;
  xCircleIcon = XCircle;
  creditCardIcon = CreditCard;
  fileTextIcon = FileText;
  searchIcon = Search;
  filterIcon = Filter;
  chevronLeftIcon = ChevronLeft;
  chevronRightIcon = ChevronRight;
  xIcon = X;

  // Computed: Filtered orders
  filteredOrders = computed(() => {
    let orders = this.allOrders();

    // Search filter
    const search = this.searchTerm().toLowerCase().trim();
    if (search) {
      orders = orders.filter(order => 
        order.orderNumber.toLowerCase().includes(search) ||
        (order.tableNumber && order.tableNumber.toLowerCase().includes(search))
      );
    }

    // Status filter
    const status = this.statusFilter();
    if (status !== null) {
      orders = orders.filter(order => {
        const orderStatus = typeof order.status === 'number' ? order.status : order.status;
        return orderStatus === status;
      });
    }

    // Table filter
    const tableFilter = this.tableFilter();
    if (tableFilter === 'table') {
      orders = orders.filter(order => !order.isTakeAway && order.tableNumber);
    } else if (tableFilter === 'takeaway') {
      orders = orders.filter(order => order.isTakeAway);
    }

    return orders;
  });

  // Computed: Paginated orders
  paginatedOrders = computed(() => {
    const filtered = this.filteredOrders();
    const page = this.currentPage();
    const size = this.pageSize();
    const start = (page - 1) * size;
    const end = start + size;
    return filtered.slice(start, end);
  });

  // Computed: Total pages
  totalPages = computed(() => {
    const filtered = this.filteredOrders();
    return Math.ceil(filtered.length / this.pageSize());
  });

  // Computed: Pagination info
  paginationInfo = computed(() => {
    const filtered = this.filteredOrders();
    const page = this.currentPage();
    const size = this.pageSize();
    const start = filtered.length === 0 ? 0 : (page - 1) * size + 1;
    const end = Math.min(page * size, filtered.length);
    return { start, end, total: filtered.length };
  });

  // Status options for filter
  statusOptions = [
    { value: null, label: 'All Statuses' },
    { value: OrderStatus.Draft, label: 'Draft' },
    { value: OrderStatus.Submitted, label: 'Submitted' },
    { value: OrderStatus.InPreparation, label: 'In Preparation' },
    { value: OrderStatus.Ready, label: 'Ready' },
    { value: OrderStatus.Delivered, label: 'Delivered' },
    { value: OrderStatus.Cancelled, label: 'Cancelled' },
    { value: OrderStatus.Paid, label: 'Paid' }
  ];

  ngOnInit(): void {
    this.loadOrders();
    // Refresh when navigating back to this page
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      if (event.url === '/orders' || event.urlAfterRedirects === '/orders') {
        this.loadOrders();
      }
    });
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.apiService.get<Order[]>('Orders').subscribe({
      next: (orders) => {
        this.allOrders.set(orders);
        this.isLoading.set(false);
        // Reset to first page when loading new data
        this.currentPage.set(1);
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        this.toastService.error('Failed to load orders');
        this.isLoading.set(false);
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.currentPage.set(1); // Reset to first page when filtering
  }

  onStatusFilterChange(value: number | null): void {
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }

  onTableFilterChange(value: 'all' | 'table' | 'takeaway'): void {
    this.tableFilter.set(value);
    this.currentPage.set(1);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    const total = this.totalPages();
    if (page >= 1 && page <= total) {
      this.currentPage.set(page);
    }
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.currentPage() + 1);
    }
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set(null);
    this.tableFilter.set('all');
    this.currentPage.set(1);
  }

  hasActiveFilters = computed(() => {
    return this.searchTerm().trim() !== '' || 
           this.statusFilter() !== null || 
           this.tableFilter() !== 'all';
  });

  getStatusText(status: number | OrderStatus): string {
    const statusMap: Record<number, string> = {
      [OrderStatus.Draft]: 'Draft',
      [OrderStatus.Submitted]: 'Submitted',
      [OrderStatus.InPreparation]: 'In Preparation',
      [OrderStatus.Ready]: 'Ready',
      [OrderStatus.Delivered]: 'Delivered',
      [OrderStatus.Cancelled]: 'Cancelled',
      [OrderStatus.Paid]: 'Paid'
    };
    return statusMap[status] || 'Unknown';
  }

  getStatusIcon(status: number | OrderStatus) {
    const statusNum = typeof status === 'number' ? status : status;
    const icons: Record<number, any> = {
      [OrderStatus.Draft]: this.fileTextIcon,
      [OrderStatus.Submitted]: this.checkCircleIcon,
      [OrderStatus.InPreparation]: this.chefHatIcon,
      [OrderStatus.Ready]: this.clockIcon,
      [OrderStatus.Delivered]: this.truckIcon,
      [OrderStatus.Cancelled]: this.xCircleIcon,
      [OrderStatus.Paid]: this.creditCardIcon
    };
    return icons[statusNum] || this.fileTextIcon;
  }

  getStatusBadgeClasses(status: number | OrderStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const classes: Record<number, string> = {
      [OrderStatus.Draft]: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700',
      [OrderStatus.Submitted]: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-700',
      [OrderStatus.InPreparation]: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-700',
      [OrderStatus.Ready]: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-700',
      [OrderStatus.Delivered]: 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-700',
      [OrderStatus.Cancelled]: 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 border-red-200 dark:border-red-700',
      [OrderStatus.Paid]: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-700'
    };
    return classes[statusNum] || 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      month: '2-digit',
      day: '2-digit',
      year: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    }).format(date);
  }

  // Generate page numbers for pagination
  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    if (total <= 7) {
      // Show all pages if 7 or fewer
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      // Show first page, current page, last page, and pages around current
      if (current <= 3) {
        // Near the start
        for (let i = 1; i <= 4; i++) pages.push(i);
        pages.push(-1); // Ellipsis
        pages.push(total);
      } else if (current >= total - 2) {
        // Near the end
        pages.push(1);
        pages.push(-1); // Ellipsis
        for (let i = total - 3; i <= total; i++) pages.push(i);
      } else {
        // In the middle
        pages.push(1);
        pages.push(-1); // Ellipsis
        for (let i = current - 1; i <= current + 1; i++) pages.push(i);
        pages.push(-1); // Ellipsis
        pages.push(total);
      }
    }
    
    return pages;
  }
}

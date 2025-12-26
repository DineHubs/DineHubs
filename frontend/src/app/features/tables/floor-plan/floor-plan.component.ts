import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Plus, Settings, RefreshCw, Grid3x3, ChevronDown } from 'lucide-angular';
import { TableService } from '../../../core/services/table.service';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { Table, TableStatus } from '../../../core/models/table.model';
import { AppRoles } from '../../../core/constants/roles.constants';

interface Branch {
  id: string;
  name: string;
  location: string;
}

@Component({
  selector: 'app-floor-plan',
  standalone: true,
  imports: [
    CommonModule,
    LucideAngularModule
  ],
  templateUrl: './floor-plan.component.html',
  styleUrl: './floor-plan.component.scss'
})
export class FloorPlanComponent implements OnInit {
  private tableService = inject(TableService);
  private authService = inject(AuthService);
  private apiService = inject(ApiService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  // State
  tables = signal<Table[]>([]);
  branches = signal<Branch[]>([]);
  selectedBranchId = signal<string | null>(null);
  isLoading = signal<boolean>(false);
  isDragging = signal<boolean>(false);
  draggedTable = signal<Table | null>(null);
  showBranchDropdown = signal<boolean>(false);

  // Icons
  plusIcon = Plus;
  settingsIcon = Settings;
  refreshIcon = RefreshCw;
  gridIcon = Grid3x3;
  chevronDownIcon = ChevronDown;

  // Role checks
  isAdmin = computed(() => this.authService.hasRole(AppRoles.Admin));
  isManager = computed(() => this.authService.hasRole(AppRoles.Manager));
  isWaiter = computed(() => this.authService.hasRole(AppRoles.Waiter));

  // Can change status: Admin and Manager only
  canChangeStatus = computed(() => this.isAdmin() || this.isManager());

  // Can configure: Admin only
  canConfigure = computed(() => this.isAdmin());

  // Show branch selector: Admin only
  showBranchSelector = computed(() => this.isAdmin());

  // Selected branch name for display
  selectedBranchName = computed(() => {
    const branchId = this.selectedBranchId();
    if (!branchId) return 'All Branches';
    const branch = this.branches().find(b => b.id === branchId);
    return branch?.name ?? 'Select Branch';
  });

  // Table status counts
  availableTablesCount = computed(() => 
    this.tables().filter(t => t.status === TableStatus.Available).length
  );
  
  occupiedTablesCount = computed(() => 
    this.tables().filter(t => t.status === TableStatus.Occupied).length
  );
  
  reservedTablesCount = computed(() => 
    this.tables().filter(t => t.status === TableStatus.Reserved).length
  );

  ngOnInit(): void {
    if (this.isAdmin()) {
      this.loadBranches();
    }
    this.loadTables();
  }

  loadBranches(): void {
    this.apiService.get<Branch[]>('Branches').subscribe({
      next: (branches) => {
        this.branches.set(branches);
        // Auto-select first branch if available
        if (branches.length > 0 && !this.selectedBranchId()) {
          this.selectedBranchId.set(branches[0].id);
          this.loadTables();
        }
      },
      error: (err) => {
        console.error('Error loading branches:', err);
        this.toastService.error('Failed to load branches');
      }
    });
  }

  loadTables(): void {
    this.isLoading.set(true);
    const branchId = this.selectedBranchId() ?? undefined;

    this.tableService.getTables(branchId).subscribe({
      next: (tables) => {
        this.tables.set(tables);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading tables:', err);
        this.toastService.error('Failed to load tables');
        this.isLoading.set(false);
      }
    });
  }

  selectBranch(branchId: string): void {
    this.selectedBranchId.set(branchId);
    this.showBranchDropdown.set(false);
    this.loadTables();
  }

  toggleBranchDropdown(): void {
    this.showBranchDropdown.update(v => !v);
  }

  closeBranchDropdown(): void {
    this.showBranchDropdown.set(false);
  }

  getTableStatusClass(status: TableStatus): string {
    switch (status) {
      case TableStatus.Available:
        return 'table-available';
      case TableStatus.Occupied:
        return 'table-occupied';
      case TableStatus.Reserved:
        return 'table-reserved';
      default:
        return 'table-available';
    }
  }

  getTableStatusBorderColor(status: TableStatus): string {
    switch (status) {
      case TableStatus.Available:
        return 'border-emerald-500';
      case TableStatus.Occupied:
        return 'border-amber-500';
      case TableStatus.Reserved:
        return 'border-blue-500';
      default:
        return 'border-gray-300';
    }
  }

  getTableStatusBgColor(status: TableStatus): string {
    switch (status) {
      case TableStatus.Available:
        return 'bg-emerald-50 dark:bg-emerald-900/20';
      case TableStatus.Occupied:
        return 'bg-amber-50 dark:bg-amber-900/20';
      case TableStatus.Reserved:
        return 'bg-blue-50 dark:bg-blue-900/20';
      default:
        return 'bg-gray-50 dark:bg-gray-900/20';
    }
  }

  onTableClick(table: Table): void {
    if (table.status === TableStatus.Available) {
      // Navigate to order creation with table info
      this.router.navigate(['/orders/create'], {
        queryParams: {
          tableId: table.id,
          tableNumber: table.tableNumber
        }
      });
    } else if (this.canChangeStatus()) {
      // Show status change options for Admin/Manager
      this.cycleTableStatus(table);
    }
  }

  cycleTableStatus(table: Table): void {
    if (!this.canChangeStatus()) {
      return;
    }

    let newStatus: TableStatus;
    switch (table.status) {
      case TableStatus.Available:
        newStatus = TableStatus.Occupied;
        break;
      case TableStatus.Occupied:
        newStatus = TableStatus.Reserved;
        break;
      case TableStatus.Reserved:
        newStatus = TableStatus.Available;
        break;
      default:
        newStatus = TableStatus.Available;
    }

    this.tableService.updateTableStatus(table.id, { status: newStatus }).subscribe({
      next: (updatedTable) => {
        this.tables.update(tables =>
          tables.map(t => t.id === updatedTable.id ? updatedTable : t)
        );
        this.toastService.success(`Table ${table.tableNumber} status updated to ${updatedTable.statusName}`);
      },
      error: (err) => {
        console.error('Error updating table status:', err);
        this.toastService.error('Failed to update table status');
      }
    });
  }

  setTableStatus(table: Table, status: TableStatus): void {
    if (!this.canChangeStatus()) {
      return;
    }

    this.tableService.updateTableStatus(table.id, { status }).subscribe({
      next: (updatedTable) => {
        this.tables.update(tables =>
          tables.map(t => t.id === updatedTable.id ? updatedTable : t)
        );
        this.toastService.success(`Table ${table.tableNumber} set to ${updatedTable.statusName}`);
      },
      error: (err) => {
        console.error('Error updating table status:', err);
        this.toastService.error('Failed to update table status');
      }
    });
  }

  // Drag and drop for Admin only
  onDragStart(event: DragEvent, table: Table): void {
    if (!this.canConfigure()) {
      event.preventDefault();
      return;
    }
    this.isDragging.set(true);
    this.draggedTable.set(table);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragEnd(): void {
    this.isDragging.set(false);
    this.draggedTable.set(null);
  }

  onDragOver(event: DragEvent): void {
    if (this.canConfigure()) {
      event.preventDefault();
    }
  }

  onDrop(event: DragEvent): void {
    if (!this.canConfigure()) return;

    event.preventDefault();
    const table = this.draggedTable();
    if (!table) return;

    const canvas = (event.target as HTMLElement).closest('.floor-canvas');
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left - (table.width / 2);
    const y = event.clientY - rect.top - (table.height / 2);

    // Clamp to canvas bounds
    const clampedX = Math.max(0, Math.min(x, rect.width - table.width));
    const clampedY = Math.max(0, Math.min(y, rect.height - table.height));

    this.tableService.updateTable(table.id, {
      tableNumber: table.tableNumber,
      positionX: clampedX,
      positionY: clampedY,
      width: table.width,
      height: table.height
    }).subscribe({
      next: (updatedTable) => {
        this.tables.update(tables =>
          tables.map(t => t.id === updatedTable.id ? updatedTable : t)
        );
      },
      error: (err) => {
        console.error('Error updating table position:', err);
        this.toastService.error('Failed to update table position');
      }
    });

    this.isDragging.set(false);
    this.draggedTable.set(null);
  }

  navigateToTableManagement(): void {
    this.router.navigate(['/tables/manage']);
  }

  refreshTables(): void {
    this.loadTables();
  }

  // Expose TableStatus enum to template
  TableStatus = TableStatus;
}

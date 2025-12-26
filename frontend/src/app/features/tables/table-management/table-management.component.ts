import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, Plus, Trash2, Edit, ChevronDown, ArrowLeft, Grid3x3, X, Save } from 'lucide-angular';
import { TableService } from '../../../core/services/table.service';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { Table, TableStatus } from '../../../core/models/table.model';

interface Branch {
  id: string;
  name: string;
  location: string;
}

interface TableForm {
  tableNumber: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
}

@Component({
  selector: 'app-table-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule
  ],
  templateUrl: './table-management.component.html',
  styleUrl: './table-management.component.scss'
})
export class TableManagementComponent implements OnInit {
  private tableService = inject(TableService);
  private apiService = inject(ApiService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  // State
  tables = signal<Table[]>([]);
  branches = signal<Branch[]>([]);
  selectedBranchId = signal<string | null>(null);
  isLoading = signal<boolean>(false);
  showBranchDropdown = signal<boolean>(false);

  // Modals
  showAddModal = signal<boolean>(false);
  showBulkAddModal = signal<boolean>(false);
  showEditModal = signal<boolean>(false);
  showDeleteModal = signal<boolean>(false);

  // Form state
  selectedTable = signal<Table | null>(null);
  tableForm = signal<TableForm>({
    tableNumber: '',
    positionX: 0,
    positionY: 0,
    width: 100,
    height: 100
  });
  bulkCount = signal<number>(5);

  // Constants
  readonly MIN_TABLES = 2;
  readonly MAX_TABLES = 50;

  // Icons
  plusIcon = Plus;
  trashIcon = Trash2;
  editIcon = Edit;
  chevronDownIcon = ChevronDown;
  arrowLeftIcon = ArrowLeft;
  gridIcon = Grid3x3;
  xIcon = X;
  saveIcon = Save;

  // Computed
  selectedBranchName = computed(() => {
    const branchId = this.selectedBranchId();
    if (!branchId) return 'Select Branch';
    const branch = this.branches().find(b => b.id === branchId);
    return branch?.name ?? 'Select Branch';
  });

  tableCount = computed(() => this.tables().length);

  canAddMore = computed(() => this.tableCount() < this.MAX_TABLES);

  canDelete = computed(() => this.tableCount() > this.MIN_TABLES);

  remainingSlots = computed(() => this.MAX_TABLES - this.tableCount());

  ngOnInit(): void {
    this.loadBranches();
  }

  loadBranches(): void {
    this.isLoading.set(true);
    this.apiService.get<Branch[]>('Branches').subscribe({
      next: (branches) => {
        this.branches.set(branches);
        if (branches.length > 0) {
          this.selectedBranchId.set(branches[0].id);
          this.loadTables();
        } else {
          this.isLoading.set(false);
        }
      },
      error: (err) => {
        console.error('Error loading branches:', err);
        this.toastService.error('Failed to load branches');
        this.isLoading.set(false);
      }
    });
  }

  loadTables(): void {
    const branchId = this.selectedBranchId();
    if (!branchId) return;

    this.isLoading.set(true);
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

  // Add single table
  openAddModal(): void {
    if (!this.canAddMore()) {
      this.toastService.error(`Maximum ${this.MAX_TABLES} tables per branch`);
      return;
    }
    this.tableForm.set({
      tableNumber: '',
      positionX: 0,
      positionY: 0,
      width: 100,
      height: 100
    });
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.resetForm();
  }

  createTable(): void {
    const branchId = this.selectedBranchId();
    const form = this.tableForm();
    if (!branchId || !form.tableNumber.trim()) return;

    this.tableService.createTable({
      branchId,
      tableNumber: form.tableNumber.trim(),
      positionX: form.positionX,
      positionY: form.positionY,
      width: form.width,
      height: form.height
    }).subscribe({
      next: (table) => {
        this.tables.update(tables => [...tables, table]);
        this.toastService.success(`Table ${table.tableNumber} created`);
        this.closeAddModal();
      },
      error: (err) => {
        console.error('Error creating table:', err);
        const message = err.error?.message || 'Failed to create table';
        this.toastService.error(message);
      }
    });
  }

  // Bulk add tables
  openBulkAddModal(): void {
    if (!this.canAddMore()) {
      this.toastService.error(`Maximum ${this.MAX_TABLES} tables per branch`);
      return;
    }
    this.bulkCount.set(Math.min(5, this.remainingSlots()));
    this.showBulkAddModal.set(true);
  }

  closeBulkAddModal(): void {
    this.showBulkAddModal.set(false);
  }

  updateBulkCount(value: number): void {
    const clamped = Math.max(this.MIN_TABLES, Math.min(value, this.remainingSlots()));
    this.bulkCount.set(clamped);
  }

  bulkCreateTables(): void {
    const branchId = this.selectedBranchId();
    if (!branchId) return;

    const count = this.bulkCount();
    if (count < this.MIN_TABLES || count > this.remainingSlots()) {
      this.toastService.error(`Please select between ${this.MIN_TABLES} and ${this.remainingSlots()} tables`);
      return;
    }

    this.tableService.bulkCreateTables({ branchId, count }).subscribe({
      next: (tables) => {
        this.tables.update(existing => [...existing, ...tables]);
        this.toastService.success(`${tables.length} tables created`);
        this.closeBulkAddModal();
      },
      error: (err) => {
        console.error('Error bulk creating tables:', err);
        const message = err.error?.message || 'Failed to create tables';
        this.toastService.error(message);
      }
    });
  }

  // Edit table
  openEditModal(table: Table): void {
    this.selectedTable.set(table);
    this.tableForm.set({
      tableNumber: table.tableNumber,
      positionX: table.positionX,
      positionY: table.positionY,
      width: table.width,
      height: table.height
    });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedTable.set(null);
    this.resetForm();
  }

  updateTable(): void {
    const table = this.selectedTable();
    const form = this.tableForm();
    if (!table || !form.tableNumber.trim()) return;

    this.tableService.updateTable(table.id, {
      tableNumber: form.tableNumber.trim(),
      positionX: form.positionX,
      positionY: form.positionY,
      width: form.width,
      height: form.height
    }).subscribe({
      next: (updatedTable) => {
        this.tables.update(tables =>
          tables.map(t => t.id === updatedTable.id ? updatedTable : t)
        );
        this.toastService.success(`Table ${updatedTable.tableNumber} updated`);
        this.closeEditModal();
      },
      error: (err) => {
        console.error('Error updating table:', err);
        const message = err.error?.message || 'Failed to update table';
        this.toastService.error(message);
      }
    });
  }

  // Delete table
  openDeleteModal(table: Table): void {
    if (!this.canDelete()) {
      this.toastService.error(`Minimum ${this.MIN_TABLES} tables required per branch`);
      return;
    }
    this.selectedTable.set(table);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.selectedTable.set(null);
  }

  confirmDelete(): void {
    const table = this.selectedTable();
    if (!table) return;

    this.tableService.deleteTable(table.id).subscribe({
      next: () => {
        this.tables.update(tables => tables.filter(t => t.id !== table.id));
        this.toastService.success(`Table ${table.tableNumber} deleted`);
        this.closeDeleteModal();
      },
      error: (err) => {
        console.error('Error deleting table:', err);
        const message = err.error?.message || 'Failed to delete table';
        this.toastService.error(message);
      }
    });
  }

  // Form helpers
  resetForm(): void {
    this.tableForm.set({
      tableNumber: '',
      positionX: 0,
      positionY: 0,
      width: 100,
      height: 100
    });
  }

  updateFormField(field: keyof TableForm, value: string | number): void {
    this.tableForm.update(form => ({ ...form, [field]: value }));
  }

  // Navigation
  goBack(): void {
    this.router.navigate(['/tables']);
  }

  // Status helpers
  getStatusClass(status: TableStatus): string {
    switch (status) {
      case TableStatus.Available:
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-800 dark:text-emerald-200';
      case TableStatus.Occupied:
        return 'bg-amber-100 text-amber-700 dark:bg-amber-800 dark:text-amber-200';
      case TableStatus.Reserved:
        return 'bg-blue-100 text-blue-700 dark:bg-blue-800 dark:text-blue-200';
      default:
        return 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-200';
    }
  }
}


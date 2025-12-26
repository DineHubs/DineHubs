import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Plus, Search, Edit, Trash2, Printer, ChevronLeft, ChevronRight, X, Check, Wifi, Usb, TestTube } from 'lucide-angular';
import { PrinterConfigurationService } from '../../../core/services/printer-configuration.service';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  PrinterConfiguration,
  PrinterType,
  ConnectionType,
  PrinterTypeLabels,
  ConnectionTypeLabels,
  PaperWidthOptions,
  CreatePrinterConfigurationRequest,
  UpdatePrinterConfigurationRequest
} from '../../../core/models/printer-configuration.model';

interface Branch {
  id: string;
  name: string;
}

interface PrinterForm {
  branchId: string;
  name: string;
  type: PrinterType;
  connectionType: ConnectionType;
  printerName: string;
  paperWidth: number;
  isDefault: boolean;
  isActive: boolean;
}

@Component({
  selector: 'app-printer-management',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    FormsModule,
    LucideAngularModule
  ],
  templateUrl: './printer-management.component.html',
  styleUrl: './printer-management.component.scss'
})
export class PrinterManagementComponent implements OnInit {
  private printerService = inject(PrinterConfigurationService);
  private apiService = inject(ApiService);
  private toastService = inject(ToastService);

  printers = signal<PrinterConfiguration[]>([]);
  branches = signal<Branch[]>([]);
  searchTerm = signal<string>('');
  filterBranchId = signal<string>('');
  filterType = signal<string>('');
  isLoading = signal<boolean>(false);
  showAddModal = signal<boolean>(false);
  showEditModal = signal<boolean>(false);
  showDeleteModal = signal<boolean>(false);
  selectedPrinter = signal<PrinterConfiguration | null>(null);
  isTesting = signal<string | null>(null);

  printerForm = signal<PrinterForm>({
    branchId: '',
    name: '',
    type: PrinterType.Receipt,
    connectionType: ConnectionType.USB,
    printerName: '',
    paperWidth: 80,
    isDefault: false,
    isActive: true
  });

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [5, 10, 25, 100];

  // Icons
  plusIcon = Plus;
  searchIcon = Search;
  editIcon = Edit;
  trashIcon = Trash2;
  printerIcon = Printer;
  chevronLeftIcon = ChevronLeft;
  chevronRightIcon = ChevronRight;
  xIcon = X;
  checkIcon = Check;
  wifiIcon = Wifi;
  usbIcon = Usb;
  testIcon = TestTube;

  // Enums for template
  PrinterType = PrinterType;
  ConnectionType = ConnectionType;
  PrinterTypeLabels = PrinterTypeLabels;
  ConnectionTypeLabels = ConnectionTypeLabels;
  PaperWidthOptions = PaperWidthOptions;

  printerTypes = [
    { value: PrinterType.Kitchen, label: 'Kitchen' },
    { value: PrinterType.Receipt, label: 'Receipt' },
    { value: PrinterType.Label, label: 'Label' }
  ];

  connectionTypes = [
    { value: ConnectionType.USB, label: 'USB' },
    { value: ConnectionType.Network, label: 'Network' },
    { value: ConnectionType.Serial, label: 'Serial' }
  ];

  filteredPrinters = computed(() => {
    let result = this.printers();
    const term = this.searchTerm().toLowerCase();
    const branchId = this.filterBranchId();
    const type = this.filterType();

    if (term) {
      result = result.filter(p =>
        p.name.toLowerCase().includes(term) ||
        p.printerName.toLowerCase().includes(term) ||
        p.branchName.toLowerCase().includes(term)
      );
    }

    if (branchId) {
      result = result.filter(p => p.branchId === branchId);
    }

    if (type) {
      result = result.filter(p => p.type === +type);
    }

    return result;
  });

  totalPages = computed(() => Math.ceil(this.filteredPrinters().length / this.pageSize()));
  paginatedPrinters = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return this.filteredPrinters().slice(start, end);
  });

  paginationInfo = computed(() => {
    const total = this.filteredPrinters().length;
    const start = total > 0 ? (this.currentPage() - 1) * this.pageSize() + 1 : 0;
    const end = Math.min(start + this.pageSize() - 1, total);
    return { start, end, total };
  });

  ngOnInit(): void {
    this.loadBranches();
    this.loadPrinters();
  }

  loadBranches(): void {
    this.apiService.get<Branch[]>('Branches').subscribe({
      next: (data) => {
        this.branches.set(data);
        // Set default branch for form if available
        if (data.length > 0 && !this.printerForm().branchId) {
          this.updateFormField('branchId', data[0].id);
        }
      },
      error: (err) => {
        console.error('Error loading branches', err);
      }
    });
  }

  loadPrinters(): void {
    this.isLoading.set(true);
    this.printerService.getAll().subscribe({
      next: (data) => {
        this.printers.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading printers', err);
        this.toastService.error('Failed to load printers');
        this.isLoading.set(false);
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.currentPage.set(1);
  }

  onBranchFilterChange(value: string): void {
    this.filterBranchId.set(value);
    this.currentPage.set(1);
  }

  onTypeFilterChange(value: string): void {
    this.filterType.set(value);
    this.currentPage.set(1);
  }

  openAddModal(): void {
    const defaultBranchId = this.branches().length > 0 ? this.branches()[0].id : '';
    this.printerForm.set({
      branchId: defaultBranchId,
      name: '',
      type: PrinterType.Receipt,
      connectionType: ConnectionType.USB,
      printerName: '',
      paperWidth: 80,
      isDefault: false,
      isActive: true
    });
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
  }

  openEditModal(printer: PrinterConfiguration): void {
    this.selectedPrinter.set(printer);
    this.printerForm.set({
      branchId: printer.branchId,
      name: printer.name,
      type: printer.type,
      connectionType: printer.connectionType,
      printerName: printer.printerName,
      paperWidth: printer.paperWidth,
      isDefault: printer.isDefault,
      isActive: printer.isActive
    });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedPrinter.set(null);
  }

  openDeleteModal(printer: PrinterConfiguration): void {
    this.selectedPrinter.set(printer);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.selectedPrinter.set(null);
  }

  updateFormField(field: keyof PrinterForm, value: any): void {
    const current = this.printerForm();
    this.printerForm.set({ ...current, [field]: value });
  }

  createPrinter(): void {
    const form = this.printerForm();
    if (!form.branchId || !form.name.trim() || !form.printerName.trim()) return;

    const request: CreatePrinterConfigurationRequest = {
      branchId: form.branchId,
      name: form.name.trim(),
      type: form.type,
      connectionType: form.connectionType,
      printerName: form.printerName.trim(),
      paperWidth: form.paperWidth,
      isDefault: form.isDefault
    };

    this.printerService.create(request).subscribe({
      next: () => {
        this.toastService.success('Printer created successfully');
        this.closeAddModal();
        this.loadPrinters();
      },
      error: (err) => {
        console.error('Error creating printer', err);
        const errorMessage = err.error?.message || 'Failed to create printer';
        this.toastService.error(errorMessage);
      }
    });
  }

  updatePrinter(): void {
    const printer = this.selectedPrinter();
    const form = this.printerForm();
    if (!printer || !form.name.trim() || !form.printerName.trim()) return;

    const request: UpdatePrinterConfigurationRequest = {
      name: form.name.trim(),
      type: form.type,
      connectionType: form.connectionType,
      printerName: form.printerName.trim(),
      paperWidth: form.paperWidth,
      isDefault: form.isDefault,
      isActive: form.isActive
    };

    this.printerService.update(printer.id, request).subscribe({
      next: () => {
        this.toastService.success('Printer updated successfully');
        this.closeEditModal();
        this.loadPrinters();
      },
      error: (err) => {
        console.error('Error updating printer', err);
        const errorMessage = err.error?.message || 'Failed to update printer';
        this.toastService.error(errorMessage);
      }
    });
  }

  confirmDelete(): void {
    const printer = this.selectedPrinter();
    if (!printer) return;

    this.printerService.delete(printer.id).subscribe({
      next: () => {
        this.toastService.success('Printer deleted successfully');
        this.closeDeleteModal();
        this.loadPrinters();
      },
      error: (err) => {
        console.error('Error deleting printer', err);
        const errorMessage = err.error?.message || 'Failed to delete printer';
        this.toastService.error(errorMessage);
      }
    });
  }

  testPrinter(printer: PrinterConfiguration): void {
    this.isTesting.set(printer.id);
    this.printerService.testPrinter(printer.id).subscribe({
      next: (result) => {
        this.toastService.success(result.message);
        this.isTesting.set(null);
      },
      error: (err) => {
        console.error('Error testing printer', err);
        const errorMessage = err.error?.message || 'Failed to test printer';
        this.toastService.error(errorMessage);
        this.isTesting.set(null);
      }
    });
  }

  getConnectionIcon(connectionType: ConnectionType) {
    switch (connectionType) {
      case ConnectionType.Network:
        return this.wifiIcon;
      case ConnectionType.USB:
      case ConnectionType.Serial:
      default:
        return this.usbIcon;
    }
  }

  getTypeColor(type: PrinterType): string {
    switch (type) {
      case PrinterType.Kitchen:
        return 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300';
      case PrinterType.Receipt:
        return 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300';
      case PrinterType.Label:
        return 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300';
      default:
        return 'bg-gray-100 dark:bg-gray-900/30 text-gray-700 dark:text-gray-300';
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
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

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  getPageNumbers(): (number | -1)[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: (number | -1)[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1);
      if (current > 2 && current < total - 1) pages.push(current - 1);
      if (current > 1 && current < total) pages.push(current);
      if (current < total - 1 && current > 2) pages.push(current + 1);
      if (current < total - 2) pages.push(-1);
      pages.push(total);
    }
    return [...new Set(pages)].sort((a, b) => a - b);
  }
}


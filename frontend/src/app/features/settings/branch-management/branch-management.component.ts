import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { LucideAngularModule, Plus, Search, MapPin, Edit, Trash2, Building2, ChevronLeft, ChevronRight, X } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';

interface Branch {
  id: string;
  name: string;
  location: string;
}

@Component({
  selector: 'app-branch-management',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    LucideAngularModule
  ],
  templateUrl: './branch-management.component.html',
  styleUrl: './branch-management.component.scss'
})
export class BranchManagementComponent implements OnInit {
  private apiService = inject(ApiService);
  private toastService = inject(ToastService);

  branches = signal<Branch[]>([]);
  searchTerm = signal<string>('');
  isLoading = signal<boolean>(false);
  showAddModal = signal<boolean>(false);
  showEditModal = signal<boolean>(false);
  showDeleteModal = signal<boolean>(false);
  selectedBranch = signal<Branch | null>(null);
  branchForm = signal<{ name: string; location: string }>({ name: '', location: '' });

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [5, 10, 25, 100];

  // Icons
  plusIcon = Plus;
  searchIcon = Search;
  mapPinIcon = MapPin;
  editIcon = Edit;
  trashIcon = Trash2;
  buildingIcon = Building2;
  chevronLeftIcon = ChevronLeft;
  chevronRightIcon = ChevronRight;
  xIcon = X;

  filteredBranches = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.branches();
    return this.branches().filter(branch =>
      branch.name.toLowerCase().includes(term) ||
      branch.location.toLowerCase().includes(term)
    );
  });

  totalPages = computed(() => Math.ceil(this.filteredBranches().length / this.pageSize()));
  paginatedBranches = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return this.filteredBranches().slice(start, end);
  });

  paginationInfo = computed(() => {
    const total = this.filteredBranches().length;
    const start = total > 0 ? (this.currentPage() - 1) * this.pageSize() + 1 : 0;
    const end = Math.min(start + this.pageSize() - 1, total);
    return { start, end, total };
  });

  ngOnInit(): void {
    this.loadBranches();
  }

  loadBranches(): void {
    this.isLoading.set(true);
    this.apiService.get<Branch[]>('Branches').subscribe({
      next: (data) => {
        this.branches.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading branches', err);
        this.toastService.error('Failed to load branches');
        this.isLoading.set(false);
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.currentPage.set(1);
  }

  openAddModal(): void {
    this.branchForm.set({ name: '', location: '' });
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.branchForm.set({ name: '', location: '' });
  }

  openEditModal(branch: Branch): void {
    this.selectedBranch.set(branch);
    this.branchForm.set({ name: branch.name, location: branch.location });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedBranch.set(null);
    this.branchForm.set({ name: '', location: '' });
  }

  openDeleteModal(branch: Branch): void {
    this.selectedBranch.set(branch);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.selectedBranch.set(null);
  }

  updateBranchName(value: string): void {
    const current = this.branchForm();
    this.branchForm.set({ ...current, name: value });
  }

  updateBranchLocation(value: string): void {
    const current = this.branchForm();
    this.branchForm.set({ ...current, location: value });
  }

  createBranch(): void {
    const form = this.branchForm();
    if (!form.name.trim() || !form.location.trim()) return;

    this.apiService.post('Branches', form).subscribe({
      next: () => {
        this.toastService.success('Branch created successfully');
        this.closeAddModal();
        this.loadBranches();
      },
      error: (err) => {
        console.error('Error creating branch', err);
        const errorMessage = err.error?.message || 'Failed to create branch';
        this.toastService.error(errorMessage);
      }
    });
  }

  updateBranch(): void {
    const branch = this.selectedBranch();
    const form = this.branchForm();
    if (!branch || !form.name.trim() || !form.location.trim()) return;

    this.apiService.put(`Branches/${branch.id}`, form).subscribe({
      next: () => {
        this.toastService.success('Branch updated successfully');
        this.closeEditModal();
        this.loadBranches();
      },
      error: (err) => {
        console.error('Error updating branch', err);
        const errorMessage = err.error?.message || 'Failed to update branch';
        this.toastService.error(errorMessage);
      }
    });
  }

  confirmDelete(): void {
    const branch = this.selectedBranch();
    if (!branch) return;

    this.apiService.delete(`Branches/${branch.id}`).subscribe({
      next: () => {
        this.toastService.success('Branch deleted successfully');
        this.closeDeleteModal();
        this.loadBranches();
      },
      error: (err) => {
        console.error('Error deleting branch', err);
        const errorMessage = err.error?.message || 'Failed to delete branch';
        this.toastService.error(errorMessage);
      }
    });
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

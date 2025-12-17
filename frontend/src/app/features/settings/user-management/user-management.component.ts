import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { LucideAngularModule, Plus, Search, Edit, Trash2, Users, Building2, Mail, ChevronLeft, ChevronRight, X } from 'lucide-angular';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { AppRoles } from '../../../core/constants/roles.constants';

interface User {
  id: string;
  email: string;
  role: string;
  branchId: string | null;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    LucideAngularModule
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  users = signal<User[]>([]);
  tenants = signal<any[]>([]);
  searchTerm = signal<string>('');
  isLoading = signal<boolean>(false);
  isSuperAdmin = signal<boolean>(false);
  selectedTenantId = signal<string | null>(null);
  contextLabel = signal<string>('');
  
  showAddModal = signal<boolean>(false);
  showEditModal = signal<boolean>(false);
  showDeleteModal = signal<boolean>(false);
  selectedUser = signal<User | null>(null);
  userForm = signal<{ email: string; password: string; role: string; branchId: string | null }>({
    email: '',
    password: '',
    role: '',
    branchId: null
  });
  branches = signal<any[]>([]);
  availableRoles = signal<string[]>([]);

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [5, 10, 25, 100];

  // Icons
  plusIcon = Plus;
  searchIcon = Search;
  editIcon = Edit;
  trashIcon = Trash2;
  usersIcon = Users;
  buildingIcon = Building2;
  mailIcon = Mail;
  chevronLeftIcon = ChevronLeft;
  chevronRightIcon = ChevronRight;
  xIcon = X;

  filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.users();
    return this.users().filter(user =>
      user.email.toLowerCase().includes(term) ||
      user.role.toLowerCase().includes(term)
    );
  });

  totalPages = computed(() => Math.ceil(this.filteredUsers().length / this.pageSize()));
  paginatedUsers = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return this.filteredUsers().slice(start, end);
  });

  paginationInfo = computed(() => {
    const total = this.filteredUsers().length;
    const start = total > 0 ? (this.currentPage() - 1) * this.pageSize() + 1 : 0;
    const end = Math.min(start + this.pageSize() - 1, total);
    return { start, end, total };
  });

  ngOnInit(): void {
    this.isSuperAdmin.set(this.authService.isSuperAdmin());
    if (this.isSuperAdmin()) {
      this.loadTenants();
      this.contextLabel.set('Select a tenant to view users');
    } else {
      this.loadUsers();
      this.contextLabel.set('Users in your tenant');
      this.loadBranches();
      this.setAvailableRoles();
    }
  }

  loadTenants(): void {
    this.apiService.get<any[]>('Tenants').subscribe({
      next: (data) => {
        this.tenants.set(data);
      },
      error: (err) => {
        console.error('Error loading tenants', err);
        this.toastService.error('Failed to load tenants');
      }
    });
  }

  onTenantSelected(tenantId: string | null): void {
    this.selectedTenantId.set(tenantId);
    if (tenantId) {
      this.loadUsersForTenant(tenantId);
      const selectedTenant = this.tenants().find(t => t.id === tenantId);
      this.contextLabel.set(selectedTenant ? `Users in ${selectedTenant.name}` : 'Users');
      this.loadBranches();
      this.setAvailableRoles();
    } else {
      this.users.set([]);
      this.contextLabel.set('Select a tenant to view users');
    }
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.apiService.get<User[]>('Users').subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading users', err);
        this.toastService.error('Failed to load users');
        this.isLoading.set(false);
      }
    });
  }

  loadUsersForTenant(tenantId: string): void {
    this.isLoading.set(true);
    this.apiService.get<User[]>(`Tenants/${tenantId}/users`).subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading users for tenant', err);
        this.toastService.error('Failed to load users');
        this.isLoading.set(false);
      }
    });
  }

  loadBranches(): void {
    this.apiService.get<any[]>('Branches').subscribe({
      next: (data) => {
        this.branches.set(data);
      },
      error: (err) => {
        console.error('Error loading branches', err);
      }
    });
  }

  setAvailableRoles(): void {
    if (this.isSuperAdmin()) {
      this.availableRoles.set([
        AppRoles.SuperAdmin,
        AppRoles.Admin,
        AppRoles.Manager,
        AppRoles.Waiter,
        AppRoles.Kitchen,
        AppRoles.InventoryManager
      ]);
    } else {
      this.availableRoles.set([
        AppRoles.Manager,
        AppRoles.Waiter,
        AppRoles.Kitchen,
        AppRoles.InventoryManager
      ]);
    }
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.currentPage.set(1);
  }

  openAddModal(): void {
    if (this.isSuperAdmin() && !this.selectedTenantId()) {
      this.toastService.error('Please select a tenant first');
      return;
    }
    this.userForm.set({ email: '', password: '', role: '', branchId: null });
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.userForm.set({ email: '', password: '', role: '', branchId: null });
  }

  openEditModal(user: User): void {
    this.selectedUser.set(user);
    this.userForm.set({ email: user.email, password: '', role: user.role, branchId: user.branchId });
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedUser.set(null);
    this.userForm.set({ email: '', password: '', role: '', branchId: null });
  }

  openDeleteModal(user: User): void {
    this.selectedUser.set(user);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.selectedUser.set(null);
  }

  updateEmail(value: string): void {
    const current = this.userForm();
    this.userForm.set({ ...current, email: value });
  }

  updatePassword(value: string): void {
    const current = this.userForm();
    this.userForm.set({ ...current, password: value });
  }

  updateRole(value: string): void {
    const current = this.userForm();
    this.userForm.set({ ...current, role: value });
  }

  updateBranchId(value: string): void {
    const current = this.userForm();
    this.userForm.set({ ...current, branchId: value === '' ? null : value });
  }

  createUser(): void {
    const form = this.userForm();
    if (!form.email.trim() || !form.role || (this.showAddModal() && !form.password.trim())) return;

    this.apiService.post('Users', form).subscribe({
      next: () => {
        this.toastService.success('User created successfully');
        this.closeAddModal();
        if (this.isSuperAdmin() && this.selectedTenantId()) {
          this.loadUsersForTenant(this.selectedTenantId()!);
        } else {
          this.loadUsers();
        }
      },
      error: (err) => {
        console.error('Error creating user', err);
        const errorMessage = err.error?.message || 'Failed to create user';
        this.toastService.error(errorMessage);
      }
    });
  }

  updateUser(): void {
    const user = this.selectedUser();
    const form = this.userForm();
    if (!user || !form.email.trim() || !form.role) return;

    const updateData: any = { email: form.email, role: form.role, branchId: form.branchId };
    if (form.password.trim()) {
      updateData.password = form.password;
    }

    this.apiService.put(`Users/${user.id}`, updateData).subscribe({
      next: () => {
        this.toastService.success('User updated successfully');
        this.closeEditModal();
        if (this.isSuperAdmin() && this.selectedTenantId()) {
          this.loadUsersForTenant(this.selectedTenantId()!);
        } else {
          this.loadUsers();
        }
      },
      error: (err) => {
        console.error('Error updating user', err);
        const errorMessage = err.error?.message || 'Failed to update user';
        this.toastService.error(errorMessage);
      }
    });
  }

  confirmDelete(): void {
    const user = this.selectedUser();
    if (!user) return;

    this.apiService.delete(`Users/${user.id}`).subscribe({
      next: () => {
        this.toastService.success('User deleted successfully');
        this.closeDeleteModal();
        if (this.isSuperAdmin() && this.selectedTenantId()) {
          this.loadUsersForTenant(this.selectedTenantId()!);
        } else {
          this.loadUsers();
        }
      },
      error: (err) => {
        console.error('Error deleting user', err);
        const errorMessage = err.error?.message || 'Failed to delete user';
        this.toastService.error(errorMessage);
      }
    });
  }

  getRoleBadgeClasses(role: string): string {
    const classes: Record<string, string> = {
      [AppRoles.SuperAdmin]: 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-700',
      [AppRoles.Admin]: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-700',
      [AppRoles.Manager]: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-700',
      [AppRoles.Waiter]: 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-300 border-yellow-200 dark:border-yellow-700',
      [AppRoles.Kitchen]: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-700',
      [AppRoles.InventoryManager]: 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 border-indigo-200 dark:border-indigo-700'
    };
    return classes[role] || 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700';
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

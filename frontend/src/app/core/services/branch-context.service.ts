import { Injectable, signal, inject, computed } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { AppRoles } from '../constants/roles.constants';

export interface Branch {
  id: string;
  name: string;
  code: string;
  location: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class BranchContextService {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  
  private readonly BRANCH_KEY = 'selected_branch';
  
  // Available branches for the current tenant
  branches = signal<Branch[]>([]);
  
  // Currently selected branch (null means "All Branches")
  selectedBranch = signal<Branch | null>(null);
  
  // Loading state
  isLoading = signal<boolean>(false);
  
  // Computed: Check if user can select branches (Admin role only)
  canSelectBranch = computed(() => {
    return this.authService.hasRole(AppRoles.Admin);
  });
  
  // Computed: Get selected branch ID (null for "All Branches")
  selectedBranchId = computed(() => {
    return this.selectedBranch()?.id ?? null;
  });
  
  // Computed: Get selected branch code
  selectedBranchCode = computed(() => {
    return this.selectedBranch()?.code ?? null;
  });

  constructor() {
    this.loadSelectedBranchFromStorage();
  }

  /**
   * Load branches for the current tenant
   */
  loadBranches(): Observable<Branch[]> {
    this.isLoading.set(true);
    
    return new Observable(subscriber => {
      this.apiService.get<Branch[]>('Branches').subscribe({
        next: (branches) => {
          this.branches.set(branches);
          this.isLoading.set(false);
          
          // Validate selected branch still exists
          const selected = this.selectedBranch();
          if (selected && !branches.find(b => b.id === selected.id)) {
            this.selectBranch(null);
          }
          
          subscriber.next(branches);
          subscriber.complete();
        },
        error: (error) => {
          console.error('Error loading branches:', error);
          this.isLoading.set(false);
          this.branches.set([]);
          subscriber.error(error);
        }
      });
    });
  }

  /**
   * Select a branch (or null for "All Branches")
   */
  selectBranch(branch: Branch | null): void {
    this.selectedBranch.set(branch);
    
    if (branch) {
      sessionStorage.setItem(this.BRANCH_KEY, JSON.stringify(branch));
    } else {
      sessionStorage.removeItem(this.BRANCH_KEY);
    }
  }

  /**
   * Select branch by ID
   */
  selectBranchById(branchId: string | null): void {
    if (!branchId) {
      this.selectBranch(null);
      return;
    }
    
    const branch = this.branches().find(b => b.id === branchId);
    if (branch) {
      this.selectBranch(branch);
    }
  }

  /**
   * Load selected branch from session storage
   */
  private loadSelectedBranchFromStorage(): void {
    try {
      const stored = sessionStorage.getItem(this.BRANCH_KEY);
      if (stored) {
        const branch = JSON.parse(stored) as Branch;
        this.selectedBranch.set(branch);
      }
    } catch {
      sessionStorage.removeItem(this.BRANCH_KEY);
    }
  }

  /**
   * Clear branch selection (reset to "All Branches")
   */
  clearSelection(): void {
    this.selectBranch(null);
  }
}


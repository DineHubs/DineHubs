import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Plus, Search, Grid3x3, List, Edit, Trash2, Power, PowerOff, Utensils } from 'lucide-angular';
import { MatDialog } from '@angular/material/dialog';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { AppRoles } from '../../../core/constants/roles.constants';
import { MenuItem } from '../../../core/models/menu-item.model';
import { MenuItemFormComponent } from '../menu-item-form/menu-item-form.component';

@Component({
  selector: 'app-menu-list',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    LucideAngularModule
  ],
  templateUrl: './menu-list.component.html',
  styleUrl: './menu-list.component.scss'
})
export class MenuListComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private toastService = inject(ToastService);
  private router = inject(Router);

  menuItems = signal<MenuItem[]>([]);
  filteredItems = signal<MenuItem[]>([]);
  categories = signal<string[]>([]);
  selectedCategory = signal<string>('All');
  searchTerm = signal<string>('');
  isLoading = signal<boolean>(false);
  viewMode = signal<'grid' | 'list'>('grid');
  showDeleteModal = signal<boolean>(false);
  itemToDelete = signal<MenuItem | null>(null);

  // Icons
  plusIcon = Plus;
  searchIcon = Search;
  gridIcon = Grid3x3;
  listIcon = List;
  editIcon = Edit;
  trashIcon = Trash2;
  powerIcon = Power;
  powerOffIcon = PowerOff;
  utensilsIcon = Utensils;

  ngOnInit(): void {
    this.loadMenuItems();
  }

  loadMenuItems(): void {
    this.isLoading.set(true);
    this.apiService.get<MenuItem[]>('MenuItems').subscribe({
      next: (items) => {
        this.menuItems.set(items);
        this.filteredItems.set(items);
        this.categories.set(['All', ...new Set(items.map(item => item.category))]);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading menu items:', error);
        if (error.status === 401) {
          this.toastService.error('Session expired. Please login again');
          this.authService.logout();
        } else if (error.status === 403) {
          this.toastService.error('You do not have permission to access menu items');
          this.router.navigate(['/dashboard']);
        } else {
          this.toastService.error('Failed to load menu items');
        }
        this.isLoading.set(false);
      }
    });
  }

  filterByCategory(category: string): void {
    this.selectedCategory.set(category);
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = this.menuItems();

    if (this.selectedCategory() !== 'All') {
      filtered = filtered.filter(item => item.category === this.selectedCategory());
    }

    const searchTerm = this.searchTerm().toLowerCase();
    if (searchTerm) {
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(searchTerm) ||
        item.category.toLowerCase().includes(searchTerm)
      );
    }

    this.filteredItems.set(filtered);
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(MenuItemFormComponent, {
      width: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadMenuItems();
      }
    });
  }

  openEditDialog(item: MenuItem): void {
    const dialogRef = this.dialog.open(MenuItemFormComponent, {
      width: '600px',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadMenuItems();
      }
    });
  }

  openDeleteModal(item: MenuItem): void {
    this.itemToDelete.set(item);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.itemToDelete.set(null);
  }

  confirmDelete(): void {
    const item = this.itemToDelete();
    if (!item) return;

    this.apiService.delete(`MenuItems/${item.id}`).subscribe({
      next: () => {
        this.toastService.success('Menu item deleted successfully');
        this.closeDeleteModal();
        this.loadMenuItems();
      },
      error: (error) => {
        console.error('Error deleting menu item:', error);
        this.toastService.error('Failed to delete menu item');
      }
    });
  }

  toggleAvailability(item: MenuItem): void {
    const updatedItem = { ...item, isAvailable: !item.isAvailable };
    this.apiService.put(`MenuItems/${item.id}`, {
      name: updatedItem.name,
      category: updatedItem.category,
      price: updatedItem.price,
      isAvailable: updatedItem.isAvailable,
      imageUrl: updatedItem.imageUrl
    }).subscribe({
      next: () => {
        this.toastService.success(`Menu item ${updatedItem.isAvailable ? 'enabled' : 'disabled'}`);
        this.loadMenuItems();
      },
      error: (error) => {
        console.error('Error updating menu item:', error);
        this.toastService.error('Failed to update menu item');
      }
    });
  }
}

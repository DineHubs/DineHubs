import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AppRoles } from '../../../core/constants/roles.constants';
import { MenuItem } from '../../../core/models/menu-item.model';
import { MenuItemFormComponent } from '../menu-item-form/menu-item-form.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
  ],
  templateUrl: './menu-list.component.html',
  styleUrl: './menu-list.component.scss'
})
export class MenuListComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  menuItems: MenuItem[] = [];
  filteredItems: MenuItem[] = [];
  categories: string[] = [];
  selectedCategory = 'All';
  searchTerm = '';
  isLoading = false;
  viewMode: 'grid' | 'list' = 'grid';

  ngOnInit(): void {
    // Access control handled by route guard
    this.loadMenuItems();
  }

  loadMenuItems(): void {
    this.isLoading = true;
    this.apiService.get<MenuItem[]>('MenuItems').subscribe({
      next: (items) => {
        this.menuItems = items;
        this.filteredItems = items;
        this.categories = ['All', ...new Set(items.map(item => item.category))];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading menu items:', error);
        if (error.status === 401) {
          this.snackBar.open('Session expired. Please login again', 'Close', { duration: 5000 });
          this.authService.logout();
        } else if (error.status === 403) {
          this.snackBar.open('You do not have permission to access menu items', 'Close', { duration: 5000 });
          this.router.navigate(['/dashboard']);
        } else {
          this.snackBar.open('Failed to load menu items', 'Close', { duration: 3000 });
        }
        this.isLoading = false;
      }
    });
  }

  filterByCategory(category: string): void {
    this.selectedCategory = category;
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = this.menuItems;

    if (this.selectedCategory !== 'All') {
      filtered = filtered.filter(item => item.category === this.selectedCategory);
    }

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(term) ||
        item.category.toLowerCase().includes(term)
      );
    }

    this.filteredItems = filtered;
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

  deleteItem(item: MenuItem): void {
    if (confirm(`Are you sure you want to delete "${item.name}"?`)) {
      this.apiService.delete(`MenuItems/${item.id}`).subscribe({
        next: () => {
          this.snackBar.open('Menu item deleted successfully', 'Close', { duration: 3000 });
          this.loadMenuItems();
        },
        error: (error) => {
          console.error('Error deleting menu item:', error);
          this.snackBar.open('Failed to delete menu item', 'Close', { duration: 3000 });
        }
      });
    }
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
        this.snackBar.open(`Menu item ${updatedItem.isAvailable ? 'enabled' : 'disabled'}`, 'Close', { duration: 3000 });
        this.loadMenuItems();
      },
      error: (error) => {
        console.error('Error updating menu item:', error);
        this.snackBar.open('Failed to update menu item', 'Close', { duration: 3000 });
      }
    });
  }
}


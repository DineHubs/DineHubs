import { Component, OnInit, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { MenuItem, CreateMenuItemRequest, UpdateMenuItemRequest } from '../../../core/models/menu-item.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-menu-item-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  templateUrl: './menu-item-form.component.html',
  styleUrl: './menu-item-form.component.scss'
})
export class MenuItemFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private dialogRef = inject(MatDialogRef<MenuItemFormComponent>);
  private snackBar = inject(MatSnackBar);
  private data = inject<MenuItem | null>(MAT_DIALOG_DATA);

  menuItemForm: FormGroup;
  isEditMode = false;
  categories = ['Food', 'Drink', 'Dessert', 'Appetizer', 'Main Course', 'Beverage'];

  constructor() {
    this.menuItemForm = this.fb.group({
      name: ['', [Validators.required]],
      category: ['', [Validators.required]],
      price: [0, [Validators.required, Validators.min(0.01)]],
      isAvailable: [true],
      imageUrl: ['']
    });
  }

  ngOnInit(): void {
    if (this.data) {
      this.isEditMode = true;
      this.menuItemForm.patchValue({
        name: this.data.name,
        category: this.data.category,
        price: this.data.price,
        isAvailable: this.data.isAvailable,
        imageUrl: this.data.imageUrl || ''
      });
    }
  }

  onSubmit(): void {
    if (this.menuItemForm.valid) {
      const formValue = this.menuItemForm.value;
      
      if (this.isEditMode && this.data) {
        const updateRequest: UpdateMenuItemRequest = {
          name: formValue.name,
          category: formValue.category,
          price: formValue.price,
          isAvailable: formValue.isAvailable,
          imageUrl: formValue.imageUrl || undefined
        };

        this.apiService.put(`MenuItems/${this.data.id}`, updateRequest).subscribe({
          next: () => {
            this.snackBar.open('Menu item updated successfully', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          },
          error: (error) => {
            console.error('Error updating menu item:', error);
            this.snackBar.open('Failed to update menu item', 'Close', { duration: 3000 });
          }
        });
      } else {
        const createRequest: CreateMenuItemRequest = {
          branchId: '', // Will be set from tenant context on backend
          name: formValue.name,
          category: formValue.category,
          price: formValue.price,
          isAvailable: formValue.isAvailable,
          imageUrl: formValue.imageUrl || undefined
        };

        this.apiService.post('MenuItems', createRequest).subscribe({
          next: () => {
            this.snackBar.open('Menu item created successfully', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          },
          error: (error) => {
            console.error('Error creating menu item:', error);
            this.snackBar.open('Failed to create menu item', 'Close', { duration: 3000 });
          }
        });
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}


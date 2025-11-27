import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../../core/services/api.service';
import { AppRoles } from '../../../../core/constants/roles.constants';

@Component({
    selector: 'app-user-dialog',
    standalone: true,
    imports: [CommonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, FormsModule],
    templateUrl: './user-dialog.component.html',
    styleUrl: './user-dialog.component.scss'
})
export class UserDialogComponent implements OnInit {
    roles: string[] = [];
    branches: any[] = [];
    private apiService = inject(ApiService);

    constructor(@Inject(MAT_DIALOG_DATA) public data: any) { }

    ngOnInit(): void {
        // If SuperAdmin, show all roles; if Admin, restrict to Manager, Waiter, Kitchen, InventoryManager
        if (this.data.isSuperAdmin) {
            this.roles = [
                AppRoles.SuperAdmin,
                AppRoles.Admin,
                AppRoles.Manager,
                AppRoles.Waiter,
                AppRoles.Kitchen,
                AppRoles.InventoryManager
            ];
        } else {
            this.roles = [
                AppRoles.Manager,
                AppRoles.Waiter,
                AppRoles.Kitchen,
                AppRoles.InventoryManager
            ];
        }
        this.loadBranches();
    }

    loadBranches(): void {
        this.apiService.get<any[]>('Branches').subscribe({
            next: (data) => this.branches = data,
            error: (err) => console.error('Error loading branches', err)
        });
    }
}

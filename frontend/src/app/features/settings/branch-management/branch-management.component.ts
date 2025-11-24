import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { BranchDialogComponent } from './branch-dialog/branch-dialog.component';

@Component({
    selector: 'app-branch-management',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        MatDialogModule,
        MatSnackBarModule,
        MatFormFieldModule,
        MatInputModule,
        MatPaginatorModule,
        MatSortModule,
        FormsModule
    ],
    templateUrl: './branch-management.component.html',
    styleUrl: './branch-management.component.scss'
})
export class BranchManagementComponent implements OnInit {
    private apiService = inject(ApiService);
    private dialog = inject(MatDialog);
    private snackBar = inject(MatSnackBar);

    dataSource = new MatTableDataSource<any>([]);
    displayedColumns = ['name', 'location', 'actions'];

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    ngOnInit(): void {
        this.loadBranches();
    }

    loadBranches(): void {
        this.apiService.get<any[]>('Branches').subscribe({
            next: (data) => {
                this.dataSource.data = data;
                this.dataSource.paginator = this.paginator;
                this.dataSource.sort = this.sort;
            },
            error: (err) => console.error('Error loading branches', err)
        });
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();

        if (this.dataSource.paginator) {
            this.dataSource.paginator.firstPage();
        }
    }

    openAddDialog(): void {
        const dialogRef = this.dialog.open(BranchDialogComponent, {
            width: '400px',
            data: { name: '', location: '' }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.createBranch(result);
            }
        });
    }

    createBranch(branch: any): void {
        this.apiService.post('Branches', branch).subscribe({
            next: () => {
                this.snackBar.open('Branch created successfully', 'Close', { duration: 3000 });
                this.loadBranches();
            },
            error: (err) => {
                console.error('Error creating branch', err);
                this.snackBar.open(err.error || 'Failed to create branch', 'Close', { duration: 3000 });
            }
        });
    }
}

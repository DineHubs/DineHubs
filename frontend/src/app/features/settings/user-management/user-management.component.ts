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
import { MatSelectModule } from '@angular/material/select';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { AppRoles } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-user-management',
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
    MatSelectModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    FormsModule
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(ApiService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  dataSource = new MatTableDataSource<any>([]);
  displayedColumns = ['email', 'role', 'branchId', 'actions'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.apiService.get<any[]>('Users').subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      error: (err) => console.error('Error loading users', err)
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
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width: '400px',
      data: { email: '', password: '', role: '', branchId: null }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.createUser(result);
      }
    });
  }

  createUser(user: any): void {
    this.apiService.post('Users', user).subscribe({
      next: () => {
        this.snackBar.open('User created successfully', 'Close', { duration: 3000 });
        this.loadUsers();
      },
      error: (err) => {
        console.error('Error creating user', err);
        this.snackBar.open(err.error || 'Failed to create user', 'Close', { duration: 3000 });
      }
    });
  }

  getRoleColor(role: string): string {
    switch (role) {
      case AppRoles.Manager: return 'primary';
      case AppRoles.Kitchen: return 'warn';
      case AppRoles.Waiter: return 'accent';
      default: return 'default';
    }
  }
}

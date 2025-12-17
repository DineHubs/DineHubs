import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cancel-order-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>Cancel Order</h2>
    <mat-dialog-content>
      <p>Are you sure you want to cancel order {{ data.orderNumber }}?</p>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason for cancellation</mat-label>
        <textarea matInput [(ngModel)]="reason" required placeholder="Enter cancellation reason"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-button color="warn" [disabled]="!reason" (click)="onConfirm()">Confirm Cancellation</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    mat-dialog-content {
      min-width: 400px;
    }
  `]
})
export class CancelOrderDialogComponent {
  reason: string = '';

  constructor(
    public dialogRef: MatDialogRef<CancelOrderDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orderNumber: string }
  ) {}

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.reason.trim()) {
      this.dialogRef.close({ reason: this.reason.trim() });
    }
  }
}


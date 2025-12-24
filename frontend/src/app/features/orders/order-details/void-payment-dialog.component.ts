import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-void-payment-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>Void Payment</h2>
    <mat-dialog-content>
      <p>Void payment for order {{ data.orderNumber }}?</p>
      <p class="warning-text">This will cancel the payment authorization. No funds will be captured.</p>
      
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason for void</mat-label>
        <textarea matInput [(ngModel)]="reason" required placeholder="Enter reason for voiding payment"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-button color="warn" [disabled]="!reason" (click)="onConfirm()">Void Payment</button>
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
    .warning-text {
      color: #f44336;
      font-size: 14px;
      margin-bottom: 16px;
    }
  `]
})
export class VoidPaymentDialogComponent {
  reason: string = '';

  constructor(
    public dialogRef: MatDialogRef<VoidPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orderNumber: string; paymentId: string }
  ) {}

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.reason.trim()) {
      this.dialogRef.close({ 
        reason: this.reason.trim(),
        paymentId: this.data.paymentId
      });
    }
  }
}


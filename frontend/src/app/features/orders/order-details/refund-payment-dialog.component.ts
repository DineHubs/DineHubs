import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-refund-payment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatRadioModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>Refund Payment</h2>
    <mat-dialog-content>
      <p>Process refund for order {{ data.orderNumber }}?</p>
      <p class="amount-info">Original Amount: <strong>RM {{ data.originalAmount.toFixed(2) }}</strong></p>
      
      <div class="refund-type">
        <mat-radio-group [(ngModel)]="refundType">
          <mat-radio-button value="full">Full Refund</mat-radio-button>
          <mat-radio-button value="partial">Partial Refund</mat-radio-button>
        </mat-radio-group>
      </div>

      @if (refundType === 'partial') {
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Refund Amount (RM)</mat-label>
          <input matInput type="number" [(ngModel)]="refundAmount" [max]="data.originalAmount" min="0.01" step="0.01" required>
          <mat-error *ngIf="refundAmount > data.originalAmount">Amount cannot exceed original payment</mat-error>
        </mat-form-field>
      }

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason for refund</mat-label>
        <textarea matInput [(ngModel)]="reason" required placeholder="Enter refund reason"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-button color="warn" [disabled]="!isValid()" (click)="onConfirm()">Process Refund</button>
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
    .amount-info {
      margin-bottom: 16px;
      color: #666;
    }
    .refund-type {
      margin-bottom: 16px;
    }
    .refund-type mat-radio-button {
      margin-right: 16px;
    }
  `]
})
export class RefundPaymentDialogComponent {
  refundType: 'full' | 'partial' = 'full';
  refundAmount: number = 0;
  reason: string = '';

  constructor(
    public dialogRef: MatDialogRef<RefundPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orderNumber: string; paymentId: string; originalAmount: number }
  ) {
    this.refundAmount = data.originalAmount;
  }

  isValid(): boolean {
    if (!this.reason.trim()) return false;
    if (this.refundType === 'partial') {
      return this.refundAmount > 0 && this.refundAmount <= this.data.originalAmount;
    }
    return true;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.isValid()) {
      const amount = this.refundType === 'full' ? this.data.originalAmount : this.refundAmount;
      this.dialogRef.close({ 
        amount,
        reason: this.reason.trim(),
        paymentId: this.data.paymentId
      });
    }
  }
}


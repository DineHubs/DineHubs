import { Component, input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, X, CheckCircle2, AlertCircle, Info } from 'lucide-angular';
import { Toast, ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss'
})
export class ToastComponent {
  toast = input.required<Toast>();
  private toastService = inject(ToastService);

  xIcon = X;
  checkCircleIcon = CheckCircle2;
  alertCircleIcon = AlertCircle;
  infoIcon = Info;

  getIcon() {
    switch (this.toast().type) {
      case 'success':
        return this.checkCircleIcon;
      case 'error':
        return this.alertCircleIcon;
      default:
        return this.infoIcon;
    }
  }

  getColorClass() {
    switch (this.toast().type) {
      case 'success':
        return 'bg-success/10 text-success border-success/20';
      case 'error':
        return 'bg-error/10 text-error border-error/20';
      default:
        return 'bg-primary/10 text-primary border-primary/20';
    }
  }

  close(): void {
    this.toastService.remove(this.toast().id);
  }
}


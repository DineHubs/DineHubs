import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';
import { ToastComponent } from '../toast/toast.component';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule, ToastComponent],
  templateUrl: './toast-container.component.html',
  styleUrl: './toast-container.component.scss'
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}


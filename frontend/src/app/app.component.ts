import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainerComponent } from './shared/components/toast-container/toast-container.component';

import { ThermalReceiptComponent } from './shared/components/thermal-receipt/thermal-receipt.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastContainerComponent, ThermalReceiptComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'frontend';
}

import { Injectable, signal } from '@angular/core';
import { Order } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class PrintService {
  printData = signal<Order | null>(null);

  constructor() {
    if (typeof window !== 'undefined') {
      window.onafterprint = () => {
        this.printData.set(null);
      };
    }
  }

  printOrder(order: Order) {
    // Set the order data first
    this.printData.set(order);

    // Increased delay to ensure Angular change detection completes and component renders
    // 500ms provides enough time for signal updates to propagate and DOM to render
    setTimeout(() => {
      try {
        window.print();
      } catch (error) {
        console.error('Error opening print dialog:', error);
      }
    }, 500);
  }
}

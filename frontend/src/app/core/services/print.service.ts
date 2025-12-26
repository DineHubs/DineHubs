import { Injectable, signal, inject, OnDestroy } from '@angular/core';
import { Order } from '../models/order.model';
import { PrinterType } from '../models/printer-configuration.model';
import { ToastService } from './toast.service';

export interface PrintJobRequest {
  type: 'receipt' | 'kitchen' | 'test' | 'drawer';
  printerType?: PrinterType;
  printerName?: string;
  data?: any;
}

export interface PrintResult {
  success: boolean;
  message: string;
  printJobId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PrintService implements OnDestroy {
  private toastService = inject(ToastService);

  // Legacy: Used for browser-based printing fallback
  printData = signal<Order | null>(null);

  // WebSocket connection to Print Agent
  private ws: WebSocket | null = null;
  private wsUrl = 'ws://localhost:9100';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectTimeout: any = null;
  private pendingRequests = new Map<string, {
    resolve: (result: PrintResult) => void;
    reject: (error: any) => void;
  }>();

  // Connection status
  agentConnected = signal<boolean>(false);
  agentStatus = signal<string>('Disconnected');

  constructor() {
    if (typeof window !== 'undefined') {
      // Legacy browser print handler
      window.onafterprint = () => {
        this.printData.set(null);
      };

      // Connect to Print Agent
      this.connectToAgent();
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  /**
   * Connect to the Print Agent WebSocket server
   */
  private connectToAgent(): void {
    if (this.ws?.readyState === WebSocket.OPEN) {
      return;
    }

    try {
      this.agentStatus.set('Connecting...');
      this.ws = new WebSocket(this.wsUrl);

      this.ws.onopen = () => {
        console.log('Connected to Print Agent');
        this.agentConnected.set(true);
        this.agentStatus.set('Connected');
        this.reconnectAttempts = 0;
      };

      this.ws.onclose = () => {
        console.log('Disconnected from Print Agent');
        this.agentConnected.set(false);
        this.agentStatus.set('Disconnected');
        this.scheduleReconnect();
      };

      this.ws.onerror = (error) => {
        console.error('Print Agent connection error:', error);
        this.agentConnected.set(false);
        this.agentStatus.set('Error');
      };

      this.ws.onmessage = (event) => {
        this.handleMessage(event.data);
      };
    } catch (error) {
      console.error('Failed to connect to Print Agent:', error);
      this.agentConnected.set(false);
      this.agentStatus.set('Failed');
      this.scheduleReconnect();
    }
  }

  /**
   * Schedule a reconnection attempt
   */
  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      this.agentStatus.set('Max retries reached');
      return;
    }

    if (this.reconnectTimeout) {
      clearTimeout(this.reconnectTimeout);
    }

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
    this.reconnectAttempts++;
    this.agentStatus.set(`Reconnecting in ${delay / 1000}s...`);

    this.reconnectTimeout = setTimeout(() => {
      this.connectToAgent();
    }, delay);
  }

  /**
   * Disconnect from Print Agent
   */
  private disconnect(): void {
    if (this.reconnectTimeout) {
      clearTimeout(this.reconnectTimeout);
    }

    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }

    this.agentConnected.set(false);
    this.agentStatus.set('Disconnected');
  }

  /**
   * Handle incoming WebSocket messages
   */
  private handleMessage(data: string): void {
    try {
      const result: PrintResult = JSON.parse(data);

      // For now, just log the result
      // In a more complex implementation, we could match responses to requests
      if (result.success) {
        console.log('Print job completed:', result.message);
      } else {
        console.error('Print job failed:', result.message);
        this.toastService.error(`Print failed: ${result.message}`);
      }
    } catch (error) {
      console.error('Error parsing print result:', error);
    }
  }

  /**
   * Send a print job to the Print Agent
   */
  private sendPrintJob(job: PrintJobRequest): Promise<boolean> {
    return new Promise((resolve) => {
      if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
        console.warn('Print Agent not connected');
        resolve(false);
        return;
      }

      try {
        this.ws.send(JSON.stringify(job));
        resolve(true);
      } catch (error) {
        console.error('Error sending print job:', error);
        resolve(false);
      }
    });
  }

  /**
   * Print an order receipt - tries Print Agent first, falls back to browser print
   */
  async printOrder(order: Order, useSilentPrint: boolean = true): Promise<void> {
    if (useSilentPrint && this.agentConnected()) {
      // Use Print Agent for silent printing
      const job: PrintJobRequest = {
        type: 'receipt',
        printerType: PrinterType.Receipt,
        data: {
          orderNumber: order.orderNumber,
          tableNumber: order.tableNumber,
          isTakeAway: order.isTakeAway,
          createdAt: order.createdAt,
          total: order.total,
          lines: order.lines.map(line => ({
            name: line.name,
            quantity: line.quantity,
            unitPrice: line.price,
            lineTotal: line.price * line.quantity
          }))
        }
      };

      const sent = await this.sendPrintJob(job);
      if (sent) {
        this.toastService.success('Receipt sent to printer');
        return;
      }
    }

    // Fallback to browser print dialog
    this.printOrderBrowser(order);
  }

  /**
   * Print a kitchen ticket - tries Print Agent first, falls back to browser print
   */
  async printKitchenTicket(order: Order, useSilentPrint: boolean = true): Promise<void> {
    if (useSilentPrint && this.agentConnected()) {
      const job: PrintJobRequest = {
        type: 'kitchen',
        printerType: PrinterType.Kitchen,
        data: {
          orderNumber: order.orderNumber,
          tableNumber: order.tableNumber,
          isTakeAway: order.isTakeAway,
          createdAt: order.createdAt,
          lines: order.lines.map(line => ({
            name: line.name,
            quantity: line.quantity
          }))
        }
      };

      const sent = await this.sendPrintJob(job);
      if (sent) {
        this.toastService.success('Kitchen ticket sent to printer');
        return;
      }
    }

    // Fallback to browser print dialog
    this.printOrderBrowser(order);
  }

  /**
   * Send a test print to the Print Agent
   */
  async testPrint(printerName?: string): Promise<boolean> {
    if (!this.agentConnected()) {
      this.toastService.error('Print Agent is not connected');
      return false;
    }

    const job: PrintJobRequest = {
      type: 'test',
      printerName
    };

    const sent = await this.sendPrintJob(job);
    if (sent) {
      this.toastService.success('Test print sent');
    }
    return sent;
  }

  /**
   * Open cash drawer via Print Agent
   */
  async openCashDrawer(printerName?: string): Promise<boolean> {
    if (!this.agentConnected()) {
      this.toastService.error('Print Agent is not connected');
      return false;
    }

    const job: PrintJobRequest = {
      type: 'drawer',
      printerName
    };

    const sent = await this.sendPrintJob(job);
    if (sent) {
      this.toastService.success('Cash drawer opened');
    }
    return sent;
  }

  /**
   * Legacy browser-based printing (fallback)
   */
  printOrderBrowser(order: Order): void {
    // Set the order data first
    this.printData.set(order);

    // Delay to ensure Angular change detection completes
    setTimeout(() => {
      try {
        window.print();
      } catch (error) {
        console.error('Error opening print dialog:', error);
      }
    }, 500);
  }

  /**
   * Manually reconnect to Print Agent
   */
  reconnect(): void {
    this.reconnectAttempts = 0;
    this.disconnect();
    this.connectToAgent();
  }

  /**
   * Check if Print Agent is available
   */
  isAgentAvailable(): boolean {
    return this.agentConnected();
  }
}

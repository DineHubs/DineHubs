import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface KitchenTicketItem {
  name: string;
  quantity: number;
  specialInstructions?: string | null;
}

export interface KitchenTicket {
  orderNumber: string;
  tableNumber: string;
  isTakeAway: boolean;
  orderTime: string;
  items: KitchenTicketItem[];
  notes?: string | null;
}

export interface KitchenPrintResult {
  printJobId: string;
  message: string;
  ticket: KitchenTicket;
}

export interface ReprintKitchenTicketRequest {
  reason: string;
}

@Injectable({
  providedIn: 'root'
})
export class KitchenPrintService {
  private apiService = inject(ApiService);

  /**
   * Get kitchen ticket data for an order
   */
  getKitchenTicket(orderId: string): Observable<KitchenTicket> {
    return this.apiService.get<KitchenTicket>(`Kitchen/orders/${orderId}/ticket`);
  }

  /**
   * Print kitchen ticket for an order
   */
  printKitchenTicket(orderId: string): Observable<KitchenPrintResult> {
    return this.apiService.post<KitchenPrintResult>(`Kitchen/orders/${orderId}/print`, {});
  }

  /**
   * Reprint kitchen ticket with reason
   */
  reprintKitchenTicket(orderId: string, reason: string): Observable<KitchenPrintResult> {
    return this.apiService.post<KitchenPrintResult>(`Kitchen/orders/${orderId}/reprint`, { reason });
  }

  /**
   * Format and print a kitchen ticket using browser print dialog
   */
  printTicketLocal(ticket: KitchenTicket): void {
    const printWindow = window.open('', '_blank', 'width=400,height=600');
    if (!printWindow) {
      console.error('Could not open print window');
      return;
    }

    const html = this.generateTicketHtml(ticket);
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();
    
    // Wait for content to load then print
    setTimeout(() => {
      printWindow.print();
      printWindow.close();
    }, 250);
  }

  private generateTicketHtml(ticket: KitchenTicket): string {
    const orderTime = new Date(ticket.orderTime).toLocaleString();
    const itemsHtml = ticket.items.map(item => `
      <tr>
        <td style="font-weight: bold; font-size: 16px;">${item.quantity}x</td>
        <td style="font-size: 16px; padding-left: 8px;">${item.name}</td>
      </tr>
      ${item.specialInstructions ? `
        <tr>
          <td></td>
          <td style="font-size: 12px; color: #666; padding-left: 16px;">→ ${item.specialInstructions}</td>
        </tr>
      ` : ''}
    `).join('');

    return `
      <!DOCTYPE html>
      <html>
      <head>
        <title>Kitchen Ticket - ${ticket.orderNumber}</title>
        <style>
          body {
            font-family: 'Courier New', monospace;
            padding: 10px;
            max-width: 300px;
            margin: 0 auto;
          }
          .header {
            text-align: center;
            border-bottom: 2px dashed #000;
            padding-bottom: 10px;
            margin-bottom: 10px;
          }
          .order-number {
            font-size: 24px;
            font-weight: bold;
          }
          .order-type {
            font-size: 18px;
            padding: 5px 10px;
            margin: 10px 0;
            display: inline-block;
            border: 2px solid #000;
          }
          .takeaway {
            background: #000;
            color: #fff;
          }
          .table-info {
            font-size: 20px;
            font-weight: bold;
          }
          .time {
            font-size: 12px;
            color: #666;
          }
          .items {
            border-bottom: 2px dashed #000;
            padding-bottom: 10px;
            margin-bottom: 10px;
          }
          .items table {
            width: 100%;
          }
          .items td {
            padding: 5px 0;
            vertical-align: top;
          }
          .footer {
            text-align: center;
            font-size: 12px;
            color: #666;
          }
          @media print {
            body { margin: 0; padding: 5mm; }
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div class="order-number">${ticket.orderNumber}</div>
          <div class="order-type ${ticket.isTakeAway ? 'takeaway' : ''}">
            ${ticket.isTakeAway ? 'TAKEAWAY' : 'DINE-IN'}
          </div>
          ${!ticket.isTakeAway ? `<div class="table-info">Table: ${ticket.tableNumber}</div>` : ''}
          <div class="time">${orderTime}</div>
        </div>
        
        <div class="items">
          <table>
            ${itemsHtml}
          </table>
        </div>
        
        ${ticket.notes ? `<div class="notes"><strong>Notes:</strong> ${ticket.notes}</div>` : ''}
        
        <div class="footer">
          *** KITCHEN COPY ***
        </div>
      </body>
      </html>
    `;
  }
}


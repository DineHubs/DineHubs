import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrintService } from '../../../core/services/print.service';

@Component({
    selector: 'app-thermal-receipt',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './thermal-receipt.component.html',
    styleUrl: './thermal-receipt.component.scss'
})
export class ThermalReceiptComponent {
    printService = inject(PrintService);
    order = this.printService.printData;

    formatDate(dateString: string | undefined): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: true
        }).format(date);
    }
}

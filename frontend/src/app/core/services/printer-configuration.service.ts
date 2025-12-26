import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  PrinterConfiguration,
  CreatePrinterConfigurationRequest,
  UpdatePrinterConfigurationRequest
} from '../models/printer-configuration.model';

@Injectable({
  providedIn: 'root'
})
export class PrinterConfigurationService {
  private apiService = inject(ApiService);
  private endpoint = 'PrinterConfigurations';

  getAll(): Observable<PrinterConfiguration[]> {
    return this.apiService.get<PrinterConfiguration[]>(this.endpoint);
  }

  getById(id: string): Observable<PrinterConfiguration> {
    return this.apiService.get<PrinterConfiguration>(`${this.endpoint}/${id}`);
  }

  create(request: CreatePrinterConfigurationRequest): Observable<PrinterConfiguration> {
    return this.apiService.post<PrinterConfiguration>(this.endpoint, request);
  }

  update(id: string, request: UpdatePrinterConfigurationRequest): Observable<PrinterConfiguration> {
    return this.apiService.put<PrinterConfiguration>(`${this.endpoint}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  testPrinter(id: string): Observable<{ message: string }> {
    return this.apiService.post<{ message: string }>(`${this.endpoint}/${id}/test`, {});
  }
}


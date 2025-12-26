import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Table,
  CreateTableRequest,
  UpdateTableRequest,
  UpdateTableStatusRequest,
  BulkCreateTablesRequest,
  TableCountResponse
} from '../models/table.model';

@Injectable({
  providedIn: 'root'
})
export class TableService {
  private apiService = inject(ApiService);
  private readonly endpoint = 'Tables';

  /**
   * Get all tables. If branchId is provided, filters by branch.
   * For Manager/Waiter, the API automatically filters by their assigned branch.
   */
  getTables(branchId?: string): Observable<Table[]> {
    const url = branchId ? `${this.endpoint}?branchId=${branchId}` : this.endpoint;
    return this.apiService.get<Table[]>(url);
  }

  /**
   * Get a specific table by ID
   */
  getTable(id: string): Observable<Table> {
    return this.apiService.get<Table>(`${this.endpoint}/${id}`);
  }

  /**
   * Create a new table (Admin only)
   */
  createTable(request: CreateTableRequest): Observable<Table> {
    return this.apiService.post<Table>(this.endpoint, request);
  }

  /**
   * Bulk create tables for a branch (Admin only)
   */
  bulkCreateTables(request: BulkCreateTablesRequest): Observable<Table[]> {
    return this.apiService.post<Table[]>(`${this.endpoint}/bulk`, request);
  }

  /**
   * Update a table (Admin only)
   */
  updateTable(id: string, request: UpdateTableRequest): Observable<Table> {
    return this.apiService.put<Table>(`${this.endpoint}/${id}`, request);
  }

  /**
   * Update table status (Admin and Manager only)
   */
  updateTableStatus(id: string, request: UpdateTableStatusRequest): Observable<Table> {
    return this.apiService.patch<Table>(`${this.endpoint}/${id}/status`, request);
  }

  /**
   * Delete a table (Admin only)
   */
  deleteTable(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  /**
   * Get table count for a branch (Admin only)
   */
  getTableCount(branchId: string): Observable<TableCountResponse> {
    return this.apiService.get<TableCountResponse>(`${this.endpoint}/count?branchId=${branchId}`);
  }
}


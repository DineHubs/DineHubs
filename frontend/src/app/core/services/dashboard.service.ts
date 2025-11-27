import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  DashboardStats,
  SalesTrend,
  TopSellingItem,
  OrderStatusCount,
  OrderHourlyCount,
  LowStockItem,
  RevenueByDay,
  AverageOrderValue
} from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiService = inject(ApiService);

  getStats(from?: Date, to?: Date): Observable<DashboardStats> {
    let endpoint = 'Dashboard/stats';
    const params: string[] = [];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    
    if (params.length > 0) {
      endpoint += `?${params.join('&')}`;
    }
    
    return this.apiService.get<DashboardStats>(endpoint);
  }

  getSalesTrend(from: Date, to: Date): Observable<SalesTrend[]> {
    const endpoint = `Dashboard/sales-trend?from=${from.toISOString()}&to=${to.toISOString()}`;
    return this.apiService.get<SalesTrend[]>(endpoint);
  }

  getTopItems(count: number = 10, from?: Date, to?: Date): Observable<TopSellingItem[]> {
    const params: string[] = [`count=${count}`];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    
    const endpoint = `Dashboard/top-items?${params.join('&')}`;
    return this.apiService.get<TopSellingItem[]>(endpoint);
  }

  getOrdersByStatus(from?: Date, to?: Date): Observable<OrderStatusCount[]> {
    const params: string[] = [];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    
    const endpoint = params.length > 0 
      ? `Dashboard/orders-by-status?${params.join('&')}`
      : 'Dashboard/orders-by-status';
    
    return this.apiService.get<OrderStatusCount[]>(endpoint);
  }

  getOrdersByHour(date?: Date): Observable<OrderHourlyCount[]> {
    const targetDate = date || new Date();
    const endpoint = `Dashboard/orders-by-hour?date=${targetDate.toISOString()}`;
    return this.apiService.get<OrderHourlyCount[]>(endpoint);
  }

  getLowStockItems(): Observable<LowStockItem[]> {
    return this.apiService.get<LowStockItem[]>('Dashboard/low-stock');
  }

  getRevenueByDay(from: Date, to: Date): Observable<RevenueByDay[]> {
    const endpoint = `Dashboard/revenue-by-day?from=${from.toISOString()}&to=${to.toISOString()}`;
    return this.apiService.get<RevenueByDay[]>(endpoint);
  }

  getAverageOrderValueTrend(from: Date, to: Date): Observable<AverageOrderValue[]> {
    const endpoint = `Dashboard/average-order-value?from=${from.toISOString()}&to=${to.toISOString()}`;
    return this.apiService.get<AverageOrderValue[]>(endpoint);
  }
}


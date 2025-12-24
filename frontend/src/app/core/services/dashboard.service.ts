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
  AverageOrderValue,
  SubscriptionStatusCount,
  SubscriptionTrend
} from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiService = inject(ApiService);

  getStats(from?: Date, to?: Date, branchId?: string | null): Observable<DashboardStats> {
    let endpoint = 'Dashboard/stats';
    const params: string[] = [];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    if (params.length > 0) {
      endpoint += `?${params.join('&')}`;
    }
    
    return this.apiService.get<DashboardStats>(endpoint);
  }

  getSalesTrend(from: Date, to: Date, branchId?: string | null): Observable<SalesTrend[]> {
    const params: string[] = [
      `from=${from.toISOString()}`,
      `to=${to.toISOString()}`
    ];
    
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = `Dashboard/sales-trend?${params.join('&')}`;
    return this.apiService.get<SalesTrend[]>(endpoint);
  }

  getTopItems(count: number = 10, from?: Date, to?: Date, branchId?: string | null): Observable<TopSellingItem[]> {
    const params: string[] = [`count=${count}`];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = `Dashboard/top-items?${params.join('&')}`;
    return this.apiService.get<TopSellingItem[]>(endpoint);
  }

  getOrdersByStatus(from?: Date, to?: Date, branchId?: string | null): Observable<OrderStatusCount[]> {
    const params: string[] = [];
    
    if (from) {
      params.push(`from=${from.toISOString()}`);
    }
    if (to) {
      params.push(`to=${to.toISOString()}`);
    }
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = params.length > 0 
      ? `Dashboard/orders-by-status?${params.join('&')}`
      : 'Dashboard/orders-by-status';
    
    return this.apiService.get<OrderStatusCount[]>(endpoint);
  }

  getOrdersByHour(date?: Date, branchId?: string | null): Observable<OrderHourlyCount[]> {
    const targetDate = date || new Date();
    const params: string[] = [`date=${targetDate.toISOString()}`];
    
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = `Dashboard/orders-by-hour?${params.join('&')}`;
    return this.apiService.get<OrderHourlyCount[]>(endpoint);
  }

  getLowStockItems(branchId?: string | null): Observable<LowStockItem[]> {
    const endpoint = branchId 
      ? `Dashboard/low-stock?branchId=${branchId}`
      : 'Dashboard/low-stock';
    return this.apiService.get<LowStockItem[]>(endpoint);
  }

  getRevenueByDay(from: Date, to: Date, branchId?: string | null): Observable<RevenueByDay[]> {
    const params: string[] = [
      `from=${from.toISOString()}`,
      `to=${to.toISOString()}`
    ];
    
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = `Dashboard/revenue-by-day?${params.join('&')}`;
    return this.apiService.get<RevenueByDay[]>(endpoint);
  }

  getAverageOrderValueTrend(from: Date, to: Date, branchId?: string | null): Observable<AverageOrderValue[]> {
    const params: string[] = [
      `from=${from.toISOString()}`,
      `to=${to.toISOString()}`
    ];
    
    if (branchId) {
      params.push(`branchId=${branchId}`);
    }
    
    const endpoint = `Dashboard/average-order-value?${params.join('&')}`;
    return this.apiService.get<AverageOrderValue[]>(endpoint);
  }

  getSubscriptionStatusBreakdown(): Observable<SubscriptionStatusCount[]> {
    return this.apiService.get<SubscriptionStatusCount[]>('Dashboard/subscription-status');
  }

  getSubscriptionTrend(months: number = 6): Observable<SubscriptionTrend[]> {
    const endpoint = `Dashboard/subscription-trend?months=${months}`;
    return this.apiService.get<SubscriptionTrend[]>(endpoint);
  }
}

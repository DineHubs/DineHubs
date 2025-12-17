import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { User, LoginRequest, LoginResponse, RefreshTokenResponse } from '../models/user.model';

import { AppRoles } from '../constants/roles.constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiService = inject(ApiService);
  private router = inject(Router);

  private readonly TOKEN_KEY = 'access_token';
  private readonly USER_KEY = 'user_data';
  private readonly TOKEN_REFRESH_THRESHOLD_MS = 5 * 60 * 1000; // 5 minutes before expiration
  private tokenRefreshTimer?: ReturnType<typeof setTimeout>;

  currentUser = signal<User | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor() {
    this.loadUserFromStorage();
    // Check token validity periodically (every 5 minutes)
    setInterval(() => {
      if (this.isAuthenticated()) {
        this.checkTokenValidity();
      }
    }, 5 * 60 * 1000);
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.apiService.post<LoginResponse>('Auth/login', credentials).pipe(
      tap(response => {
        localStorage.setItem(this.TOKEN_KEY, response.accessToken);
        const user: User = {
          id: this.getUserIdFromToken(response.accessToken),
          email: credentials.email,
          roles: response.roles,
          tenantId: this.getTenantIdFromToken(response.accessToken),
          branchId: this.getBranchIdFromToken(response.accessToken)
        };
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        this.currentUser.set(user);
        this.isAuthenticated.set(true);
        this.startTokenRefreshTimer(response.accessToken);
      }),
      catchError(error => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.stopTokenRefreshTimer();
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return roles.some(role => user.roles.includes(role));
  }

  isSuperAdmin(): boolean {
    return this.hasRole(AppRoles.SuperAdmin);
  }

  private loadUserFromStorage(): void {
    const token = this.getToken();
    const userStr = localStorage.getItem(this.USER_KEY);

    if (token && userStr) {
      try {
        const user = JSON.parse(userStr) as User;
        if (this.isTokenValid(token)) {
          this.currentUser.set(user);
          this.isAuthenticated.set(true);
          // Start refresh timer for loaded token
          this.startTokenRefreshTimer(token);
        } else {
          this.logout();
        }
      } catch {
        this.logout();
      }
    }
  }

  private isTokenValid(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000;
      return Date.now() < exp;
    } catch {
      return false;
    }
  }

  private getUserIdFromToken(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub || payload.userId || '';
    } catch {
      return '';
    }
  }

  private getTenantIdFromToken(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.tenantId || '';
    } catch {
      return '';
    }
  }

  private getBranchIdFromToken(token: string): string | undefined {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.branchId;
    } catch {
      return undefined;
    }
  }

  private getTokenExpiration(token: string): number | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp ? payload.exp * 1000 : null; // Convert to milliseconds
    } catch {
      return null;
    }
  }

  private startTokenRefreshTimer(token: string): void {
    this.stopTokenRefreshTimer();
    
    const expiration = this.getTokenExpiration(token);
    if (!expiration) {
      return;
    }

    const now = Date.now();
    const timeUntilExpiration = expiration - now;
    const refreshTime = timeUntilExpiration - this.TOKEN_REFRESH_THRESHOLD_MS;

    if (refreshTime <= 0) {
      // Token is already expired or about to expire
      this.refreshTokenIfNeeded();
      return;
    }

    // Set timer to refresh token before expiration
    this.tokenRefreshTimer = setTimeout(() => {
      this.refreshTokenIfNeeded();
    }, refreshTime);
  }

  private stopTokenRefreshTimer(): void {
    if (this.tokenRefreshTimer) {
      clearTimeout(this.tokenRefreshTimer);
      this.tokenRefreshTimer = undefined;
    }
  }

  private refreshTokenIfNeeded(): void {
    const token = this.getToken();
    if (!token) {
      return;
    }

    const expiration = this.getTokenExpiration(token);
    if (!expiration) {
      this.logout();
      return;
    }

    const now = Date.now();
    const timeUntilExpiration = expiration - now;

    // If token expires in less than threshold, attempt refresh
    if (timeUntilExpiration < this.TOKEN_REFRESH_THRESHOLD_MS) {
      // If token is already expired, logout
      if (timeUntilExpiration <= 0) {
        console.warn('Token expired, logging out user');
        this.logout();
        return;
      }

      // Attempt to refresh the token
      this.apiService.post<RefreshTokenResponse>('Auth/refresh', { accessToken: token }).subscribe({
        next: (response) => {
          // Update token and user data
          localStorage.setItem(this.TOKEN_KEY, response.accessToken);
          const user = this.currentUser();
          if (user) {
            // Update user with new token data
            const updatedUser: User = {
              ...user,
              id: this.getUserIdFromToken(response.accessToken),
              tenantId: this.getTenantIdFromToken(response.accessToken),
              branchId: this.getBranchIdFromToken(response.accessToken),
              roles: response.roles
            };
            localStorage.setItem(this.USER_KEY, JSON.stringify(updatedUser));
            this.currentUser.set(updatedUser);
          }
          // Restart timer with new token
          this.startTokenRefreshTimer(response.accessToken);
          console.log('Token refreshed successfully');
        },
        error: (error) => {
          console.error('Token refresh failed:', error);
          // If refresh fails, logout user
          this.logout();
        }
      });
    }
  }

  /**
   * Checks if the current token is valid and not expired
   */
  checkTokenValidity(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    if (!this.isTokenValid(token)) {
      this.logout();
      return false;
    }

    // Restart refresh timer
    this.startTokenRefreshTimer(token);
    return true;
  }
}


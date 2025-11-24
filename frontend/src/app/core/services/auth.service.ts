import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { User, LoginRequest, LoginResponse } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiService = inject(ApiService);
  private router = inject(Router);
  
  private readonly TOKEN_KEY = 'access_token';
  private readonly USER_KEY = 'user_data';

  currentUser = signal<User | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor() {
    this.loadUserFromStorage();
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
      }),
      catchError(error => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  logout(): void {
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
    // SuperAdmin has access to everything
    if (user.roles.includes('SuperAdmin')) {
      return true;
    }
    return roles.some(role => user.roles.includes(role));
  }

  isSuperAdmin(): boolean {
    return this.hasRole('SuperAdmin');
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
}


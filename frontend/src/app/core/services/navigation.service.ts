import { Injectable, inject, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { NavigationMenuItem } from '../models/navigation.model';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);

  menuItems = signal<NavigationMenuItem[]>([]);

  loadMenu(): Observable<NavigationMenuItem[]> {
    return this.apiService.get<NavigationMenuItem[]>('Navigation/menu').pipe(
      tap(items => {
        const filteredItems = this.filterMenuByRoles(items);
        this.menuItems.set(this.buildMenuTree(filteredItems));
      }),
      catchError(error => {
        console.error('Error loading navigation menu:', error);
        return of([]);
      })
    );
  }

  private filterMenuByRoles(items: NavigationMenuItem[]): NavigationMenuItem[] {
    const userRoles = this.authService.currentUser()?.roles || [];
    return items.filter(item => {
      if (item.allowedRoles.length === 0) return true;
      return item.allowedRoles.some(role => userRoles.includes(role));
    });
  }

  private buildMenuTree(items: NavigationMenuItem[]): NavigationMenuItem[] {
    const itemMap = new Map<string, NavigationMenuItem>();
    const rootItems: NavigationMenuItem[] = [];

    items.forEach(item => {
      itemMap.set(item.id, { ...item, children: [] });
    });

    items.forEach(item => {
      const menuItem = itemMap.get(item.id)!;
      if (item.parentId) {
        const parent = itemMap.get(item.parentId);
        if (parent) {
          parent.children = parent.children || [];
          parent.children.push(menuItem);
        }
      } else {
        rootItems.push(menuItem);
      }
    });

    return rootItems;
  }
}


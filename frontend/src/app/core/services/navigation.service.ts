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
        // Backend already filters by roles, but we do a final client-side check for security
        // This ensures menu items are properly filtered and tree structure is maintained
        const filteredItems = this.filterMenuByRolesRecursive(items);
        this.menuItems.set(filteredItems);
      }),
      catchError(error => {
        console.error('Error loading navigation menu:', error);
        return of([]);
      })
    );
  }

  /**
   * Recursively filters menu items by user roles
   * Hides items that the user doesn't have access to
   */
  private filterMenuByRolesRecursive(items: NavigationMenuItem[]): NavigationMenuItem[] {
    const userRoles = this.authService.currentUser()?.roles || [];
    
    if (!userRoles || userRoles.length === 0) {
      return []; // No roles, no menu items
    }

    const filteredItems: NavigationMenuItem[] = [];

    for (const item of items) {
      // Check if user has access to this menu item
      const hasAccess = item.allowedRoles && item.allowedRoles.length > 0
        ? item.allowedRoles.some(role => userRoles.includes(role))
        : false; // Hide items without role restrictions for security

      if (!hasAccess) {
        continue; // Skip this item
      }

      // Recursively filter children
      let filteredChildren: NavigationMenuItem[] | undefined;
      if (item.children && item.children.length > 0) {
        filteredChildren = this.filterMenuByRolesRecursive(item.children);
        
        // If all children are filtered out and parent has no route, hide parent too
        if (filteredChildren.length === 0 && !item.route) {
          continue; // Skip this item
        }
      }

      // Add filtered item with filtered children
      filteredItems.push({
        ...item,
        children: filteredChildren && filteredChildren.length > 0 ? filteredChildren : undefined
      });
    }

    return filteredItems;
  }
}


import { Component, OnInit, OnDestroy, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { NavigationService } from '../../core/services/navigation.service';
import { NavigationMenuItem } from '../../core/models/navigation.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatListModule,
    MatIconModule,
    MatExpansionModule,
    MatDividerModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit, OnDestroy {
  private navigationService = inject(NavigationService);
  private authService = inject(AuthService);
  
  menuItems = this.navigationService.menuItems;

  constructor() {
    // Use effect to react to authentication state changes
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      
      if (isAuthenticated) {
        // Load menu when user is authenticated
        this.loadMenu();
      } else {
        // Clear menu when logged out
        this.navigationService.menuItems.set([]);
      }
    });
  }

  ngOnInit(): void {
    // Load menu on initialization if already authenticated
    if (this.authService.isAuthenticated()) {
      this.loadMenu();
    }
  }

  ngOnDestroy(): void {
    // Effect cleanup is handled automatically by Angular
  }

  private loadMenu(): void {
    this.navigationService.loadMenu().subscribe();
  }

  hasChildren(item: NavigationMenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }
}


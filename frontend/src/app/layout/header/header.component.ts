import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Menu, User, LogOut, Building2, Shield } from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    LucideAngularModule
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  @Output() menuToggle = new EventEmitter<void>();
  private authService = inject(AuthService);
  private themeService = inject(ThemeService);
  private router = inject(Router);

  currentUser = this.authService.currentUser;
  showUserMenu = signal(false);

  // Icons
  menuIcon = Menu;
  userIcon = User;
  logOutIcon = LogOut;
  buildingIcon = Building2;
  shieldIcon = Shield;

  onMenuToggle(): void {
    this.menuToggle.emit();
  }

  toggleUserMenu(): void {
    this.showUserMenu.set(!this.showUserMenu());
  }

  closeUserMenu(): void {
    this.showUserMenu.set(false);
  }

  onLogout(): void {
    this.closeUserMenu();
    this.authService.logout();
  }

  get userRoles(): string {
    return this.currentUser()?.roles?.join(', ') || 'None';
  }
}

import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Menu, User, LogOut, Building2, Shield, Moon, Sun } from 'lucide-angular';
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
  moonIcon = Moon;
  sunIcon = Sun;

  get isDarkMode(): boolean {
    return this.themeService.theme() === 'dark';
  }

  get themeIcon() {
    return this.isDarkMode ? this.sunIcon : this.moonIcon;
  }

  onMenuToggle(): void {
    this.menuToggle.emit();
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
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

import { Component, signal } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { BranchManagementComponent } from './branch-management/branch-management.component';
import { UserManagementComponent } from './user-management/user-management.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    BranchManagementComponent,
    UserManagementComponent
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent {
  activeTab = signal<'branches' | 'users'>('branches');

  setTab(tab: 'branches' | 'users'): void {
    this.activeTab.set(tab);
  }
}

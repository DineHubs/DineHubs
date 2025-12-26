import { Component, signal } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { BranchManagementComponent } from './branch-management/branch-management.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { PrinterManagementComponent } from './printer-management/printer-management.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    BranchManagementComponent,
    UserManagementComponent,
    PrinterManagementComponent
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent {
  activeTab = signal<'branches' | 'users' | 'printers'>('branches');

  setTab(tab: 'branches' | 'users' | 'printers'): void {
    this.activeTab.set(tab);
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { NavigationService } from '../../core/services/navigation.service';
import { NavigationMenuItem } from '../../core/models/navigation.model';

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
export class SidebarComponent implements OnInit {
  private navigationService = inject(NavigationService);
  
  menuItems = this.navigationService.menuItems;

  ngOnInit(): void {
    this.navigationService.loadMenu().subscribe();
  }

  hasChildren(item: NavigationMenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }
}


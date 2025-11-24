import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../../core/services/api.service';
import { MenuItem } from '../../../core/models/menu-item.model';

@Component({
  selector: 'app-customer-menu',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule
  ],
  templateUrl: './customer-menu.component.html',
  styleUrl: './customer-menu.component.scss'
})
export class CustomerMenuComponent implements OnInit {
  private apiService = inject(ApiService);

  menuItems: MenuItem[] = [];
  cart: { item: MenuItem; quantity: number }[] = [];

  ngOnInit(): void {
    this.loadMenu();
  }

  loadMenu(): void {
    // TODO: Load public menu (no auth required)
  }

  addToCart(item: MenuItem): void {
    // TODO: Implement cart functionality
  }
}


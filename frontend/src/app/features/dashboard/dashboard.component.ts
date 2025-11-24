import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    RouterModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  stats = [
    { title: 'Today\'s Orders', value: '0', icon: 'receipt', color: '#1976d2', route: '/orders' },
    { title: 'Revenue', value: 'RM 0.00', icon: 'attach_money', color: '#388e3c', route: '/reports' },
    { title: 'Active Tables', value: '0', icon: 'table_restaurant', color: '#f57c00', route: '/tables' },
    { title: 'Pending Orders', value: '0', icon: 'pending_actions', color: '#d32f2f', route: '/kitchen' }
  ];

  ngOnInit(): void {
    // Load dashboard data
  }
}


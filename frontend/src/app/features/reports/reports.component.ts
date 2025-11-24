import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  private authService = inject(AuthService);

  canAccessSubscriptionReport = false;

  ngOnInit(): void {
    // Check if user has access to subscription report
    // API requires: SuperAdmin, Admin
    this.canAccessSubscriptionReport = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
  }
}


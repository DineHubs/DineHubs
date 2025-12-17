import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppRoles } from './core/constants/roles.constants';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'menu',
        loadChildren: () => import('./features/menu/menu.routes').then(m => m.MENU_ROUTES)
      },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.routes').then(m => m.ORDERS_ROUTES)
      },
      {
        path: 'kitchen',
        canActivate: [roleGuard([AppRoles.Kitchen, AppRoles.Manager])],
        loadComponent: () => import('./features/kitchen/kitchen-display/kitchen-display.component').then(m => m.KitchenDisplayComponent)
      },
      {
        path: 'tables',
        loadChildren: () => import('./features/tables/tables.routes').then(m => m.TABLES_ROUTES)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'tenants',
        loadChildren: () => import('./features/tenants/tenants.routes').then(m => m.TENANTS_ROUTES)
      },
      {
        path: 'subscriptions',
        canActivate: [roleGuard([AppRoles.SuperAdmin])],
        loadComponent: () => import('./features/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent)
      }
    ]
  },
  {
    path: 'qr',
    loadComponent: () => import('./features/qr-ordering/customer-menu/customer-menu.component').then(m => m.CustomerMenuComponent)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];

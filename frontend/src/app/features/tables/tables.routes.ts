import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import { AppRoles } from '../../core/constants/roles.constants';

export const TABLES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./floor-plan/floor-plan.component').then(m => m.FloorPlanComponent),
    canActivate: [roleGuard([AppRoles.Admin, AppRoles.Manager, AppRoles.Waiter])]
  },
  {
    path: 'manage',
    loadComponent: () => import('./table-management/table-management.component').then(m => m.TableManagementComponent),
    canActivate: [roleGuard([AppRoles.Admin])]  // Admin only
  }
];

import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import { AppRoles } from '../../core/constants/roles.constants';

export const MENU_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard([AppRoles.Admin, AppRoles.Manager])],
    loadComponent: () => import('./menu-list/menu-list.component').then(m => m.MenuListComponent)
  }
];


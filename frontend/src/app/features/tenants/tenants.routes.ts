import { Routes } from '@angular/router';

export const TENANTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./tenant-list/tenant-list.component').then(m => m.TenantListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./tenant-create/tenant-create.component').then(m => m.TenantCreateComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./tenant-details/tenant-details.component').then(m => m.TenantDetailsComponent)
  }
];


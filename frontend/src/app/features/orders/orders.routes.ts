import { Routes } from '@angular/router';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./order-list/order-list.component').then(m => m.OrderListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./order-create/order-create.component').then(m => m.OrderCreateComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./order-details/order-details.component').then(m => m.OrderDetailsComponent)
  }
];


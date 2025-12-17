import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import { AppRoles } from '../../core/constants/roles.constants';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard([AppRoles.Manager, AppRoles.Waiter])],
    loadComponent: () => import('./order-list/order-list.component').then(m => m.OrderListComponent)
  },
  {
    path: 'active',
    canActivate: [roleGuard([AppRoles.Manager, AppRoles.Waiter])],
    loadComponent: () => import('./order-list/order-list.component').then(m => m.OrderListComponent)
  },
  {
    path: 'create',
    canActivate: [roleGuard([AppRoles.Manager, AppRoles.Waiter])],
    loadComponent: () => import('./order-create/order-create.component').then(m => m.OrderCreateComponent)
  },
  {
    path: ':id',
    canActivate: [roleGuard([AppRoles.Manager, AppRoles.Waiter])],
    loadComponent: () => import('./order-details/order-details.component').then(m => m.OrderDetailsComponent)
  },
  {
    path: ':id/payment',
    canActivate: [roleGuard([AppRoles.Manager, AppRoles.Waiter])],
    loadComponent: () => import('./order-payment/order-payment.component').then(m => m.OrderPaymentComponent)
  }
];


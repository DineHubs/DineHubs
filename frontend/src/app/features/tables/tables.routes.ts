import { Routes } from '@angular/router';

export const TABLES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./floor-plan/floor-plan.component').then(m => m.FloorPlanComponent)
  }
];


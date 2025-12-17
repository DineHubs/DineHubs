import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    if (!authService.hasAnyRole(allowedRoles)) {
      // Redirect to a safe page or show access denied
      router.navigate(['/settings'], { queryParams: { error: 'access_denied' } });
      return false;
    }

    return true;
  };
};


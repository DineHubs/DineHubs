import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  // Check token validity before making request
  // This will trigger refresh if token is about to expire
  if (!authService.checkTokenValidity()) {
    // Token is invalid, request will fail with 401
  }

  const token = authService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // If token expired (401), check if we can refresh
      // Skip refresh attempts for auth endpoints to avoid infinite loops
      if (error.status === 401 && token && !req.url.includes('Auth/refresh') && !req.url.includes('Auth/login')) {
        // Check token validity - this will attempt refresh if needed
        // If refresh fails, user will be logged out automatically
        authService.checkTokenValidity();
      }
      return throwError(() => error);
    })
  );
};


# Token Storage Strategy Documentation

## Current Implementation

The application currently stores JWT tokens in `localStorage` in the Angular frontend.

**Location:** `frontend/src/app/core/services/auth.service.ts`

### Current Approach
- **Storage Method:** `localStorage`
- **Token Key:** `access_token`
- **User Data Key:** `user_data`
- **Token Validation:** Client-side expiration check

### Security Considerations

#### Risks with localStorage:
1. **XSS Vulnerability:** Tokens stored in localStorage are accessible to any JavaScript running on the page
2. **No HttpOnly Protection:** Unlike cookies, localStorage cannot use the HttpOnly flag
3. **Persistent Storage:** Tokens persist even after browser close (unless explicitly cleared)

#### Benefits of localStorage:
1. **CSRF Protection:** localStorage is not automatically sent with requests, reducing CSRF risk
2. **Explicit Control:** Tokens are only sent when explicitly added to request headers
3. **No Cookie Size Limits:** No 4KB cookie size limitation

## Recommended Approach: HttpOnly Cookies

For enhanced security, consider migrating to httpOnly cookies:

### Implementation Steps

#### Backend Changes:
1. Modify login endpoint to set httpOnly cookie instead of returning token in response body
2. Configure CORS to allow credentials (`AllowCredentials = true`)
3. Update authentication middleware to read token from cookie instead of Authorization header

#### Frontend Changes:
1. Remove localStorage token storage
2. Update HTTP interceptor to rely on cookies (or remove token header logic)
3. Ensure CORS credentials are included in requests

### Example Backend Implementation:

```csharp
// In AuthController.Login
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true, // HTTPS only
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
};

Response.Cookies.Append("access_token", result.AccessToken, cookieOptions);
```

### Example Frontend Changes:

```typescript
// Remove localStorage usage
// Update interceptor to include credentials
// In api.service.ts or auth.interceptor.ts
req = req.clone({
  withCredentials: true // Include cookies in cross-origin requests
});
```

## Hybrid Approach (Current + Recommendations)

If keeping localStorage, implement these security measures:

1. **Content Security Policy (CSP):** Already implemented - ensure strict CSP to prevent XSS
2. **Input Sanitization:** Already implemented - continue sanitizing all user inputs
3. **Token Expiration:** Already implemented - ensure tokens expire reasonably quickly (30 minutes)
4. **Refresh Tokens:** Implement refresh token mechanism (see token refresh implementation)
5. **Automatic Logout:** Clear tokens on browser close or implement session timeout

## Token Refresh Strategy

See `quality-token-refresh` implementation for automatic token refresh before expiration.

## Migration Path

1. **Phase 1:** Implement refresh tokens with localStorage (current approach)
2. **Phase 2:** Add httpOnly cookie support alongside localStorage
3. **Phase 3:** Migrate to httpOnly cookies only (remove localStorage)

## Security Best Practices

1. **Short Token Lifetime:** Access tokens should expire quickly (15-30 minutes)
2. **Refresh Tokens:** Use longer-lived refresh tokens stored securely
3. **Token Rotation:** Rotate refresh tokens on each use
4. **Secure Transmission:** Always use HTTPS in production
5. **CSP Headers:** Maintain strict Content Security Policy
6. **Regular Audits:** Regularly review and update security measures

## Current Status

- ✅ Token expiration validation implemented
- ✅ Secure token transmission (HTTPS recommended)
- ⚠️ localStorage storage (XSS risk)
- ⚠️ No refresh token mechanism (to be implemented)
- ✅ CSP headers configured
- ✅ CORS configured with credentials support

## Recommendations

1. **Immediate:** Implement token refresh mechanism (see `quality-token-refresh`)
2. **Short-term:** Consider httpOnly cookies for production
3. **Long-term:** Implement refresh token rotation and token revocation


export interface User {
  id: string;
  email: string;
  displayName?: string;
  tenantId: string;
  branchId?: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  roles: string[];
}

export interface RefreshTokenRequest {
  accessToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  roles: string[];
}


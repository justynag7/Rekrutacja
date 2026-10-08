export interface LoginRequest {
  email: string;
  password: string;
}

export interface TokenInfo {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  displayName: string;
}

export interface RestResponse<T> {
  data: T | null;
  isSuccess: boolean;
  errorMessage: string | null;
}

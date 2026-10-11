export interface User {
  readonly id: string;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
  readonly rememberMe: boolean;
}

export interface RegisterRequest {
  readonly email: string;
  readonly password: string;
  readonly confirmPassword: string;
}

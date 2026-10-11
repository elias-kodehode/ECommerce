import { createContext, useContext } from "react";
import type { Result } from "../../lib/api/result";
import type { LoginRequest, RegisterRequest, User } from "./auth.types";

export interface AuthContextValue {
  readonly user: User | null;
  readonly isInitializing: boolean;
  readonly isLoading: boolean;
  readonly login: (request: LoginRequest) => Promise<Result<void>>;
  readonly register: (request: RegisterRequest) => Promise<Result<string>>;
  readonly logout: () => Promise<Result<void>>;
  readonly refreshUser: () => Promise<Result<User>>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}

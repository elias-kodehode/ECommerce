import { requestEmptyResult, requestResult } from "../../lib/api/client";
import type { Result } from "../../lib/api/result";
import type { LoginRequest, RegisterRequest, User } from "./auth.types";

export function loginUser(
  request: LoginRequest,
  signal?: AbortSignal,
): Promise<Result<void>> {
  return requestEmptyResult({
    method: "POST",
    url: "/users/login",
    data: request,
    signal,
  });
}

export function registerUser(
  request: RegisterRequest,
  signal?: AbortSignal,
): Promise<Result<string>> {
  return requestResult<string>(
    {
      method: "POST",
      url: "/users/register",
      data: request,
      signal,
    },
    (value): value is string => typeof value === "string",
  );
}

export function logoutUser(signal?: AbortSignal): Promise<Result<void>> {
  return requestEmptyResult({
    method: "POST",
    url: "/users/logout",
    signal,
  });
}

export function getCurrentUser(signal?: AbortSignal): Promise<Result<User>> {
  return requestResult<User>(
    {
      method: "GET",
      url: "/users/me",
      signal,
    },
    isUser,
  );
}

function isUser(value: unknown): value is User {
  return (
    typeof value === "object" &&
    value !== null &&
    !Array.isArray(value) &&
    typeof (value as Record<string, unknown>).id === "string"
  );
}

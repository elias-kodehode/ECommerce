import axios from "axios";
import type { AxiosRequestConfig, AxiosResponse } from "axios";
import {
  Result,
  type ResultError,
  type ResultErrors,
  type ResultErrorType,
} from "./result";

interface ProblemDetails {
  readonly title?: unknown;
  readonly status?: unknown;
  readonly detail?: unknown;
  readonly errors?: unknown;
}

export const apiClient = axios.create({
  baseURL: "/api",
  withCredentials: true,
  headers: {
    Accept: "application/json, application/problem+json",
  },
});

export type ResponseGuard<T> = (value: unknown) => value is T;

export function requestResult<T>(
  config: AxiosRequestConfig,
  isExpectedResponse?: ResponseGuard<T>,
): Promise<Result<T>> {
  return executeRequest(config, (response) => {
    if (response.status === 204 || response.status === 205) {
      return invalidResponseFailure<T>(response.status);
    }

    if (isExpectedResponse && !isExpectedResponse(response.data)) {
      return invalidResponseFailure<T>(response.status);
    }

    return Result.success(response.data as T);
  });
}

export function requestEmptyResult(
  config: AxiosRequestConfig,
): Promise<Result<void>> {
  return executeRequest(config, (response) =>
    response.status === 204 || response.status === 205
      ? Result.success(undefined)
      : invalidResponseFailure<void>(response.status),
  );
}

async function executeRequest<T>(
  config: AxiosRequestConfig,
  onSuccess: (response: AxiosResponse<unknown>) => Result<T>,
): Promise<Result<T>> {
  try {
    const response = await apiClient.request<unknown>(config);
    return onSuccess(response);
  } catch (error: unknown) {
    return toFailure<T>(error);
  }
}

function invalidResponseFailure<T>(status: number): Result<T> {
  return Result.failure(
    [
      {
        code: "Response.Invalid",
        description: "The server returned an unexpected response.",
        type: "failure",
      },
    ],
    { status },
  );
}

function toFailure<T>(error: unknown): Result<T> {
  if (!axios.isAxiosError(error)) {
    return Result.failure([
      {
        code: "Client.Unexpected",
        description: "An unexpected client error occurred.",
        type: "failure",
      },
    ]);
  }

  if (!error.response) {
    if (axios.isCancel(error)) {
      return Result.failure([
        {
          code: "Request.Cancelled",
          description: "The request was cancelled.",
          type: "failure",
        },
      ]);
    }

    return Result.failure([
      {
        code: "Network.Unavailable",
        description: "Unable to reach the server. Check your connection and try again.",
        type: "failure",
      },
    ]);
  }

  const status = error.response.status;
  const errorType = getErrorType(status);
  const problem = asProblemDetails(error.response.data);
  const errors = readErrors(problem?.errors, errorType);
  const title = readNonEmptyString(problem?.title);

  if (errors) {
    return Result.failure(errors, { status, title });
  }

  const description =
    readNonEmptyString(problem?.detail) ??
    title ??
    getDefaultDescription(status);

  return Result.failure(
    [
      {
        code: `Http.${status}`,
        description,
        type: errorType,
      },
    ],
    { status, title },
  );
}

function asProblemDetails(value: unknown): ProblemDetails | undefined {
  if (typeof value !== "object" || value === null || Array.isArray(value)) {
    return undefined;
  }

  return value;
}

function readErrors(
  value: unknown,
  type: ResultErrorType,
): ResultErrors | undefined {
  const errors: ResultError[] = [];

  if (Array.isArray(value)) {
    for (const item of value) {
      if (typeof item !== "object" || item === null || Array.isArray(item)) {
        continue;
      }

      const candidate = item as Record<string, unknown>;
      const code = readNonEmptyString(candidate.code);
      const description = readNonEmptyString(candidate.description);

      if (code && description) {
        errors.push({ code, description, type });
      }
    }
  } else if (typeof value === "object" && value !== null) {
    for (const [field, messages] of Object.entries(value)) {
      if (!Array.isArray(messages)) {
        continue;
      }

      const code =
        field.trim().length > 0
          ? `Validation.${field}`
          : "Validation.Error";

      for (const message of messages) {
        const description = readNonEmptyString(message);

        if (description) {
          errors.push({ code, description, type });
        }
      }
    }
  }

  if (errors.length === 0) {
    return undefined;
  }

  return [errors[0], ...errors.slice(1)];
}

function readNonEmptyString(value: unknown): string | undefined {
  if (typeof value !== "string") {
    return undefined;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : undefined;
}

function getErrorType(status: number): ResultErrorType {
  switch (status) {
    case 400:
      return "validation";
    case 401:
      return "unauthorized";
    case 403:
      return "forbidden";
    case 404:
      return "notFound";
    case 409:
      return "conflict";
    default:
      return "failure";
  }
}

function getDefaultDescription(status: number): string {
  switch (status) {
    case 400:
      return "The request was invalid.";
    case 401:
      return "Authentication is required.";
    case 403:
      return "Access is forbidden.";
    case 404:
      return "The requested resource was not found.";
    case 409:
      return "The request conflicts with the current state.";
    default:
      return "The server could not complete the request.";
  }
}

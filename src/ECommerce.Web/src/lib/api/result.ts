export type ResultErrorType =
  | "failure"
  | "validation"
  | "notFound"
  | "conflict"
  | "unauthorized"
  | "forbidden";

export interface ResultError {
  readonly code: string;
  readonly description: string;
  readonly type: ResultErrorType;
}

export type ResultErrors = readonly [ResultError, ...ResultError[]];

export interface ResultSuccess<T> {
  readonly isSuccess: true;
  readonly isFailure: false;
  readonly value: T;
  readonly errors: readonly [];
}

export interface ResultFailure {
  readonly isSuccess: false;
  readonly isFailure: true;
  readonly errors: ResultErrors;
  readonly status?: number;
  readonly title?: string;
}

export type Result<T> = ResultSuccess<T> | ResultFailure;

interface FailureMetadata {
  readonly status?: number;
  readonly title?: string;
}

export const Result = {
  success<T>(value: T): Result<T> {
    return {
      isSuccess: true,
      isFailure: false,
      value,
      errors: [],
    };
  },

  failure<T = never>(
    errors: ResultErrors,
    metadata: FailureMetadata = {},
  ): Result<T> {
    if (errors.length === 0) {
      throw new Error("A failed result must contain at least one error.");
    }

    if (
      errors.some(
        (error) =>
          error.code.trim().length === 0 ||
          error.description.trim().length === 0,
      )
    ) {
      throw new Error("Result errors must have a code and description.");
    }

    const errorType = errors[0].type;

    if (errors.some((error) => error.type !== errorType)) {
      throw new Error("All errors in a result must have the same error type.");
    }

    return {
      isSuccess: false,
      isFailure: true,
      errors,
      ...metadata,
    };
  },

  match<T, TOutput>(
    result: Result<T>,
    onSuccess: (value: T) => TOutput,
    onFailure: (
      errors: ResultErrors,
      failure: ResultFailure,
    ) => TOutput,
  ): TOutput {
    return result.isSuccess
      ? onSuccess(result.value)
      : onFailure(result.errors, result);
  },
};

export interface ApiError {
  code: string;
  description: string;
}

export type Result<T> =
  | {
      isSuccess: true;
      value: T;
    }
  | {
      isSuccess: false;
      errors: ApiError[];
      status?: number;
    };

import axios, { type AxiosRequestConfig } from "axios";
import type { ApiError, Result } from "./result";

interface ProblemDetails {
  title?: string;
  errors?: ApiError[];
}

export const apiClient = axios.create({
  baseURL: "/api",
  withCredentials: true,
});

export async function apiRequest<T>(
  config: AxiosRequestConfig,
): Promise<Result<T>> {
  try {
    const response = await apiClient.request<T>(config);

    return {
      isSuccess: true,
      value: response.data,
    };
  } catch (error) {
    if (!axios.isAxiosError<ProblemDetails>(error)) {
      return {
        isSuccess: false,
        errors: [
          {
            code: "Client.Unknown",
            description: "The request could not be completed.",
          },
        ],
      };
    }
    const problem = error.response?.data;

    return {
      isSuccess: false,
      status: error.response?.status,
      errors: problem?.errors ?? [
        {
          code: "Http.Error",
          description:
            problem?.title ??
            error.message ??
            "The request could not be completed.",
        },
      ],
    };
  }
}

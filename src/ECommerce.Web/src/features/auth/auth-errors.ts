import type { ResultError } from "@/lib/api/result";

export interface AuthErrorGroups {
  readonly email: readonly ResultError[];
  readonly password: readonly ResultError[];
  readonly confirmPassword: readonly ResultError[];
  readonly form: readonly ResultError[];
}

export function groupAuthErrors(
  errors: readonly ResultError[],
): AuthErrorGroups {
  const email: ResultError[] = [];
  const password: ResultError[] = [];
  const confirmPassword: ResultError[] = [];
  const form: ResultError[] = [];

  for (const error of errors) {
    const code = error.code.toLowerCase();

    if (code.includes("confirmpassword")) {
      confirmPassword.push(error);
    } else if (code.includes("email") || code.includes("username")) {
      email.push(error);
    } else if (code.includes("password")) {
      password.push(error);
    } else {
      form.push(error);
    }
  }

  return { email, password, confirmPassword, form };
}

export function errorMessages(
  errors: readonly ResultError[],
): Array<{ message: string }> {
  return errors.map((error) => ({ message: error.description }));
}

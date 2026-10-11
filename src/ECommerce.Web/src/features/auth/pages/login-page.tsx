import {
  useMemo,
  useRef,
  useState,
  type FormEvent,
} from "react";
import { CircleAlertIcon, CircleCheckIcon, LogInIcon } from "lucide-react";
import { Link, useLocation } from "react-router";
import { readAuthRouteState } from "@/app/auth-route-state";
import { AuthShell } from "@/components/auth/auth-shell";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import type { ResultError } from "@/lib/api/result";
import { errorMessages, groupAuthErrors } from "../auth-errors";
import { useAuth } from "../auth-context";

export function LoginPage() {
  const { isLoading, login } = useAuth();
  const location = useLocation();
  const routeState = readAuthRouteState(location.state);
  const [errors, setErrors] = useState<readonly ResultError[]>([]);
  const groupedErrors = useMemo(() => groupAuthErrors(errors), [errors]);
  const emailRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);
  const formErrorRef = useRef<HTMLDivElement>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const email = String(data.get("email") ?? "").trim();
    const password = String(data.get("password") ?? "");

    setErrors([]);

    const result = await login({
      email,
      password,
      rememberMe: data.get("rememberMe") === "true",
    });

    if (result.isFailure) {
      const nextErrors = groupAuthErrors(result.errors);
      setErrors(result.errors);

      requestAnimationFrame(() => {
        if (nextErrors.email.length > 0) {
          emailRef.current?.focus();
        } else if (nextErrors.password.length > 0) {
          passwordRef.current?.focus();
        } else {
          formErrorRef.current?.focus();
        }
      });
    }
  }

  function clearErrors() {
    if (errors.length > 0) {
      setErrors([]);
    }
  }

  return (
    <AuthShell>
      <Card className="shadow-xl shadow-black/5">
        <CardHeader className="gap-2 px-6 pt-2 sm:px-7">
          <CardTitle className="text-2xl tracking-tight">
            Welcome back
          </CardTitle>
          <CardDescription>
            Sign in to continue to your account.
          </CardDescription>
        </CardHeader>

        <CardContent className="px-6 sm:px-7">
          <form
            aria-busy={isLoading}
            className="space-y-5"
            onSubmit={handleSubmit}
          >
            {routeState.registeredEmail && (
              <Alert role="status" aria-live="polite">
                <CircleCheckIcon aria-hidden="true" />
                <AlertTitle>Account created</AlertTitle>
                <AlertDescription>
                  Sign in with {routeState.registeredEmail} to get started.
                </AlertDescription>
              </Alert>
            )}

            {groupedErrors.form.length > 0 && (
              <Alert
                ref={formErrorRef}
                tabIndex={-1}
                variant="destructive"
                aria-live="assertive"
              >
                <CircleAlertIcon aria-hidden="true" />
                <AlertTitle>We could not sign you in</AlertTitle>
                <AlertDescription>
                  <ul className="list-disc space-y-1 pl-4">
                    {groupedErrors.form.map((error, index) => (
                      <li key={`${error.code}-${index}`}>
                        {error.description}
                      </li>
                    ))}
                  </ul>
                </AlertDescription>
              </Alert>
            )}

            <FieldGroup>
              <Field data-invalid={groupedErrors.email.length > 0 || undefined}>
                <FieldLabel htmlFor="login-email">Email address</FieldLabel>
                <Input
                  ref={emailRef}
                  id="login-email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  defaultValue={routeState.registeredEmail ?? ""}
                  placeholder="you@example.com"
                  required
                  disabled={isLoading}
                  aria-invalid={groupedErrors.email.length > 0}
                  aria-describedby={
                    groupedErrors.email.length > 0
                      ? "login-email-error"
                      : undefined
                  }
                  onChange={clearErrors}
                />
                <FieldError
                  id="login-email-error"
                  errors={errorMessages(groupedErrors.email)}
                />
              </Field>

              <Field
                data-invalid={groupedErrors.password.length > 0 || undefined}
              >
                <FieldLabel htmlFor="login-password">Password</FieldLabel>
                <Input
                  ref={passwordRef}
                  id="login-password"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  required
                  disabled={isLoading}
                  aria-invalid={groupedErrors.password.length > 0}
                  aria-describedby={
                    groupedErrors.password.length > 0
                      ? "login-password-error"
                      : undefined
                  }
                  onChange={clearErrors}
                />
                <FieldError
                  id="login-password-error"
                  errors={errorMessages(groupedErrors.password)}
                />
              </Field>

              <Field orientation="horizontal">
                <Checkbox
                  id="remember-me"
                  name="rememberMe"
                  value="true"
                  disabled={isLoading}
                />
                <FieldLabel htmlFor="remember-me" className="font-normal">
                  Keep me signed in
                </FieldLabel>
              </Field>
            </FieldGroup>

            <Button
              className="h-10 w-full"
              size="lg"
              type="submit"
              disabled={isLoading}
            >
              {isLoading ? (
                <>
                  <Spinner aria-hidden="true" />
                  Signing in…
                </>
              ) : (
                <>
                  Sign in
                  <LogInIcon aria-hidden="true" data-icon="inline-end" />
                </>
              )}
            </Button>
          </form>
        </CardContent>

        <CardFooter className="justify-center py-4 text-sm text-muted-foreground">
          New to Northstar?
          <Button asChild variant="link" className="h-auto px-1.5 py-0">
            <Link to="/register" state={{ from: routeState.from }}>
              Create an account
            </Link>
          </Button>
        </CardFooter>
      </Card>
    </AuthShell>
  );
}

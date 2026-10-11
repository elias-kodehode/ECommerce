import {
  useMemo,
  useRef,
  useState,
  type FormEvent,
} from "react";
import { CircleAlertIcon, UserPlusIcon } from "lucide-react";
import { Link, useLocation, useNavigate } from "react-router";
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
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import type { ResultError } from "@/lib/api/result";
import { errorMessages, groupAuthErrors } from "../auth-errors";
import { useAuth } from "../auth-context";

export function RegisterPage() {
  const { isLoading, register } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const routeState = readAuthRouteState(location.state);
  const [errors, setErrors] = useState<readonly ResultError[]>([]);
  const [confirmationError, setConfirmationError] = useState<string>();
  const groupedErrors = useMemo(() => groupAuthErrors(errors), [errors]);
  const emailRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);
  const confirmationRef = useRef<HTMLInputElement>(null);
  const formErrorRef = useRef<HTMLDivElement>(null);
  const confirmationMessages = confirmationError
    ? [{ message: confirmationError }]
    : errorMessages(groupedErrors.confirmPassword);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const email = String(data.get("email") ?? "").trim();
    const password = String(data.get("password") ?? "");
    const confirmPassword = String(data.get("confirmPassword") ?? "");

    setErrors([]);
    setConfirmationError(undefined);

    if (password !== confirmPassword) {
      setConfirmationError("Passwords do not match.");
      requestAnimationFrame(() => confirmationRef.current?.focus());
      return;
    }

    const result = await register({ email, password, confirmPassword });

    if (result.isSuccess) {
      navigate("/login", {
        replace: true,
        state: { from: routeState.from, registeredEmail: email },
      });
      return;
    }

    const nextErrors = groupAuthErrors(result.errors);
    setErrors(result.errors);

    requestAnimationFrame(() => {
      if (nextErrors.email.length > 0) {
        emailRef.current?.focus();
      } else if (nextErrors.password.length > 0) {
        passwordRef.current?.focus();
      } else if (nextErrors.confirmPassword.length > 0) {
        confirmationRef.current?.focus();
      } else {
        formErrorRef.current?.focus();
      }
    });
  }

  function clearErrors() {
    if (errors.length > 0) {
      setErrors([]);
    }
    if (confirmationError) {
      setConfirmationError(undefined);
    }
  }

  return (
    <AuthShell>
      <Card className="shadow-xl shadow-black/5">
        <CardHeader className="gap-2 px-6 pt-2 sm:px-7">
          <CardTitle className="text-2xl tracking-tight">
            Create your account
          </CardTitle>
          <CardDescription>
            Join Northstar and keep your shopping in one place.
          </CardDescription>
        </CardHeader>

        <CardContent className="px-6 sm:px-7">
          <form
            aria-busy={isLoading}
            className="space-y-5"
            onSubmit={handleSubmit}
          >
            {groupedErrors.form.length > 0 && (
              <Alert
                ref={formErrorRef}
                tabIndex={-1}
                variant="destructive"
                aria-live="assertive"
              >
                <CircleAlertIcon aria-hidden="true" />
                <AlertTitle>We could not create your account</AlertTitle>
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
                <FieldLabel htmlFor="register-email">Email address</FieldLabel>
                <Input
                  ref={emailRef}
                  id="register-email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  placeholder="you@example.com"
                  required
                  disabled={isLoading}
                  aria-invalid={groupedErrors.email.length > 0}
                  aria-describedby={
                    groupedErrors.email.length > 0
                      ? "register-email-error"
                      : undefined
                  }
                  onChange={clearErrors}
                />
                <FieldError
                  id="register-email-error"
                  errors={errorMessages(groupedErrors.email)}
                />
              </Field>

              <Field
                data-invalid={groupedErrors.password.length > 0 || undefined}
              >
                <FieldLabel htmlFor="register-password">Password</FieldLabel>
                <Input
                  ref={passwordRef}
                  id="register-password"
                  name="password"
                  type="password"
                  autoComplete="new-password"
                  minLength={4}
                  required
                  disabled={isLoading}
                  aria-invalid={groupedErrors.password.length > 0}
                  aria-describedby={
                    groupedErrors.password.length > 0
                      ? "register-password-error register-password-hint"
                      : "register-password-hint"
                  }
                  onChange={clearErrors}
                />
                <FieldDescription id="register-password-hint">
                  Use at least 4 characters.
                </FieldDescription>
                <FieldError
                  id="register-password-error"
                  errors={errorMessages(groupedErrors.password)}
                />
              </Field>

              <Field data-invalid={confirmationMessages.length > 0 || undefined}>
                <FieldLabel htmlFor="register-password-confirmation">
                  Confirm password
                </FieldLabel>
                <Input
                  ref={confirmationRef}
                  id="register-password-confirmation"
                  name="confirmPassword"
                  type="password"
                  autoComplete="new-password"
                  minLength={4}
                  required
                  disabled={isLoading}
                  aria-invalid={confirmationMessages.length > 0}
                  aria-describedby={
                    confirmationMessages.length > 0
                      ? "register-password-confirmation-error"
                      : undefined
                  }
                  onChange={clearErrors}
                />
                <FieldError
                  id="register-password-confirmation-error"
                  errors={confirmationMessages}
                />
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
                  Creating account…
                </>
              ) : (
                <>
                  Create account
                  <UserPlusIcon aria-hidden="true" data-icon="inline-end" />
                </>
              )}
            </Button>
          </form>
        </CardContent>

        <CardFooter className="justify-center py-4 text-sm text-muted-foreground">
          Already have an account?
          <Button asChild variant="link" className="h-auto px-1.5 py-0">
            <Link to="/login" state={{ from: routeState.from }}>
              Sign in
            </Link>
          </Button>
        </CardFooter>
      </Card>
    </AuthShell>
  );
}

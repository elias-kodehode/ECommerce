import { useState } from "react";
import {
  CircleAlertIcon,
  LogOutIcon,
  ShieldCheckIcon,
  ShoppingBagIcon,
  UserRoundIcon,
} from "lucide-react";
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
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import type { ResultError } from "@/lib/api/result";
import { useAuth } from "../auth/auth-context";

export function HomePage() {
  const { isLoading, logout, user } = useAuth();
  const [errors, setErrors] = useState<readonly ResultError[]>([]);

  async function handleLogout() {
    setErrors([]);
    const result = await logout();

    if (result.isFailure) {
      setErrors(result.errors);
    }
  }

  return (
    <main className="min-h-svh bg-linear-to-b from-slate-50 to-background dark:from-slate-950 dark:to-background">
      <header className="border-b bg-background/80 backdrop-blur">
        <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-3">
            <div className="grid size-9 place-items-center rounded-xl bg-primary text-primary-foreground">
              <ShoppingBagIcon aria-hidden="true" className="size-4" />
            </div>
            <span className="font-heading font-semibold tracking-tight">
              Northstar Market
            </span>
          </div>

          <Button
            type="button"
            variant="outline"
            disabled={isLoading}
            aria-busy={isLoading}
            onClick={handleLogout}
          >
            {isLoading ? (
              <>
                <Spinner aria-hidden="true" />
                Signing out…
              </>
            ) : (
              <>
                <LogOutIcon aria-hidden="true" data-icon="inline-start" />
                Sign out
              </>
            )}
          </Button>
        </div>
      </header>

      <div className="mx-auto w-full max-w-6xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
        <div className="max-w-2xl">
          <p className="mb-3 text-sm font-medium text-muted-foreground">
            Account overview
          </p>
          <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
            Welcome to Northstar.
          </h1>
          <p className="mt-3 text-base leading-7 text-muted-foreground">
            You are signed in and ready to shop. Your protected account details
            are shown below.
          </p>
        </div>

        {errors.length > 0 && (
          <Alert
            className="mt-8 max-w-2xl"
            variant="destructive"
            aria-live="assertive"
          >
            <CircleAlertIcon aria-hidden="true" />
            <AlertTitle>We could not sign you out</AlertTitle>
            <AlertDescription>
              <ul className="list-disc space-y-1 pl-4">
                {errors.map((error, index) => (
                  <li key={`${error.code}-${index}`}>{error.description}</li>
                ))}
              </ul>
            </AlertDescription>
          </Alert>
        )}

        <div className="mt-10 grid gap-5 md:grid-cols-2">
          <Card className="shadow-sm">
            <CardHeader>
              <div className="mb-2 grid size-10 place-items-center rounded-xl bg-muted">
                <UserRoundIcon aria-hidden="true" className="size-5" />
              </div>
              <CardTitle>Your account</CardTitle>
              <CardDescription>
                The identifier attached to this signed-in session.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="rounded-lg border bg-muted/40 px-3 py-2.5 font-mono text-sm break-all">
                {user?.id ?? "Account unavailable"}
              </div>
            </CardContent>
          </Card>

          <Card className="shadow-sm">
            <CardHeader>
              <div className="mb-2 grid size-10 place-items-center rounded-xl bg-muted">
                <ShieldCheckIcon aria-hidden="true" className="size-5" />
              </div>
              <CardTitle>Secure session</CardTitle>
              <CardDescription>
                Your account route is protected and only available after
                authentication.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm font-medium">
                <span
                  aria-hidden="true"
                  className="size-2 rounded-full bg-emerald-500"
                />
                Signed in
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </main>
  );
}

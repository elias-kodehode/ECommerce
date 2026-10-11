import type { ReactNode } from "react";
import { ShoppingBagIcon } from "lucide-react";

interface AuthShellProps {
  readonly children: ReactNode;
}

export function AuthShell({ children }: AuthShellProps) {
  return (
    <main className="relative isolate flex min-h-svh overflow-hidden bg-linear-to-br from-slate-50 via-background to-stone-100 px-4 py-10 dark:from-slate-950 dark:via-background dark:to-stone-950 sm:px-6">
      <div
        aria-hidden="true"
        className="absolute -top-32 left-1/2 -z-10 size-96 -translate-x-1/2 rounded-full bg-primary/8 blur-3xl"
      />
      <div
        aria-hidden="true"
        className="absolute right-[-8rem] bottom-[-10rem] -z-10 size-80 rounded-full bg-slate-400/10 blur-3xl dark:bg-slate-300/5"
      />

      <div className="mx-auto flex w-full max-w-md flex-col justify-center">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="mb-4 grid size-11 place-items-center rounded-2xl bg-primary text-primary-foreground shadow-sm">
            <ShoppingBagIcon aria-hidden="true" className="size-5" />
          </div>
          <p className="font-heading text-lg font-semibold tracking-tight">
            Northstar Market
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            A simpler way to shop.
          </p>
        </div>

        {children}

        <p className="mt-6 text-center text-xs text-muted-foreground">
          Your session is protected with secure, cookie-based authentication.
        </p>
      </div>
    </main>
  );
}

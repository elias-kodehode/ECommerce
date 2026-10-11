import { Navigate, Outlet, useLocation } from "react-router";
import { Spinner } from "@/components/ui/spinner";
import { useAuth } from "@/features/auth/auth-context";
import {
  currentLocationPath,
  readAuthRouteState,
} from "./auth-route-state";

export function RequireAuthRoute() {
  const { isInitializing, user } = useAuth();
  const location = useLocation();

  if (isInitializing) {
    return <SessionLoadingScreen />;
  }

  if (user === null) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: currentLocationPath(location) }}
      />
    );
  }

  return <Outlet />;
}

export function GuestOnlyRoute() {
  const { isInitializing, user } = useAuth();
  const location = useLocation();

  if (isInitializing) {
    return <SessionLoadingScreen />;
  }

  if (user !== null) {
    const { from } = readAuthRouteState(location.state);
    const destination = from === currentLocationPath(location) ? "/" : from;

    return <Navigate to={destination} replace />;
  }

  return <Outlet />;
}

export function RouteFallback() {
  const { isInitializing, user } = useAuth();

  if (isInitializing) {
    return <SessionLoadingScreen />;
  }

  return <Navigate to={user === null ? "/login" : "/"} replace />;
}

export function SessionLoadingScreen() {
  return (
    <main
      className="grid min-h-svh place-items-center bg-background px-6"
      aria-busy="true"
      aria-live="polite"
    >
      <div className="flex items-center gap-3 text-sm text-muted-foreground">
        <Spinner className="size-5" />
        <span>Checking your session...</span>
      </div>
    </main>
  );
}

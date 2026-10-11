import type { Location } from "react-router";

export interface AuthRouteState {
  readonly from: string;
  readonly registeredEmail?: string;
}

const defaultRouteState: AuthRouteState = { from: "/" };

export function readAuthRouteState(state: unknown): AuthRouteState {
  if (typeof state !== "object" || state === null || Array.isArray(state)) {
    return defaultRouteState;
  }

  const candidate = state as Record<string, unknown>;
  const from = isSafeInternalPath(candidate.from) ? candidate.from : "/";
  const registeredEmail = readNonEmptyString(candidate.registeredEmail);

  return registeredEmail === undefined
    ? { from }
    : { from, registeredEmail };
}

export function currentLocationPath(
  location: Pick<Location, "pathname" | "search" | "hash">,
): string {
  return `${location.pathname}${location.search}${location.hash}`;
}

function isSafeInternalPath(value: unknown): value is string {
  return (
    typeof value === "string" &&
    value.startsWith("/") &&
    !value.startsWith("//") &&
    !value.includes("\\") &&
    !hasControlCharacter(value)
  );
}

function hasControlCharacter(value: string): boolean {
  return Array.from(value).some((character) => {
    const codePoint = character.codePointAt(0);
    return codePoint !== undefined && (codePoint < 32 || codePoint === 127);
  });
}

function readNonEmptyString(value: unknown): string | undefined {
  if (typeof value !== "string") {
    return undefined;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : undefined;
}

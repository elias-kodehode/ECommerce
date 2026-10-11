import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import type { Result } from "../../lib/api/result";
import {
  getCurrentUser,
  loginUser,
  logoutUser,
  registerUser,
} from "./auth.api";
import { AuthContext, type AuthContextValue } from "./auth-context";
import type { LoginRequest, RegisterRequest, User } from "./auth.types";

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const [pendingRequests, setPendingRequests] = useState(0);
  const authOperation = useRef(0);
  const authOperationQueue = useRef<Promise<void>>(Promise.resolve());
  const isLoading = isInitializing || pendingRequests > 0;

  const runAuthOperation = useCallback(function enqueueAuthOperation<T>(
    operation: () => Promise<T>,
  ): Promise<T> {
    const result = authOperationQueue.current.then(operation, operation);
    authOperationQueue.current = result.then(
      () => undefined,
      () => undefined,
    );
    return result;
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    const operation = ++authOperation.current;

    void getCurrentUser(controller.signal)
      .then((result) => {
        if (
          !controller.signal.aborted &&
          operation === authOperation.current &&
          result.isSuccess
        ) {
          setUser(result.value);
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsInitializing(false);
        }
      });

    return () => controller.abort();
  }, []);

  const login = useCallback(
    (request: LoginRequest): Promise<Result<void>> =>
      runAuthOperation(async () => {
        const operation = ++authOperation.current;
        setPendingRequests((count) => count + 1);

        try {
          const result = await loginUser(request);

          if (result.isFailure) {
            return result;
          }

          if (operation === authOperation.current) {
            setUser(null);
          }

          const currentUserResult = await getCurrentUser();

          if (currentUserResult.isFailure) {
            return currentUserResult;
          }

          if (operation === authOperation.current) {
            setUser(currentUserResult.value);
          }

          return result;
        } finally {
          setPendingRequests((count) => Math.max(0, count - 1));
        }
      }),
    [runAuthOperation],
  );

  const register = useCallback(
    async (request: RegisterRequest): Promise<Result<string>> => {
      setPendingRequests((count) => count + 1);

      try {
        return await registerUser(request);
      } finally {
        setPendingRequests((count) => Math.max(0, count - 1));
      }
    },
    [],
  );

  const logout = useCallback(
    (): Promise<Result<void>> =>
      runAuthOperation(async () => {
        const operation = ++authOperation.current;
        setPendingRequests((count) => count + 1);

        try {
          const result = await logoutUser();

          if (result.isSuccess && operation === authOperation.current) {
            setUser(null);
          }

          return result;
        } finally {
          setPendingRequests((count) => Math.max(0, count - 1));
        }
      }),
    [runAuthOperation],
  );

  const refreshUser = useCallback(
    (): Promise<Result<User>> =>
      runAuthOperation(async () => {
        const operation = ++authOperation.current;
        setPendingRequests((count) => count + 1);

        try {
          const result = await getCurrentUser();

          if (result.isSuccess && operation === authOperation.current) {
            setUser(result.value);
          } else if (
            result.isFailure &&
            (result.errors[0].type === "unauthorized" ||
              result.errors[0].type === "forbidden") &&
            operation === authOperation.current
          ) {
            setUser(null);
          }

          return result;
        } finally {
          setPendingRequests((count) => Math.max(0, count - 1));
        }
      }),
    [runAuthOperation],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isInitializing,
      isLoading,
      login,
      register,
      logout,
      refreshUser,
    }),
    [
      user,
      isInitializing,
      isLoading,
      login,
      register,
      logout,
      refreshUser,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

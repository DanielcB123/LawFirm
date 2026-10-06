import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { getActorProfile, loginWithPassword } from "../../services/authApi";
import type { ActorProfile, LoginResponse } from "../../types/auth";

type AuthContextValue = {
  token: string | null;
  actor: ActorProfile | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);
const TOKEN_STORAGE_KEY = "lawfirm.auth.token";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_STORAGE_KEY));
  const [actor, setActor] = useState<ActorProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function bootstrap() {
      if (!token) {
        setIsLoading(false);
        return;
      }

      try {
        const profile = await getActorProfile(token);
        setActor(profile);
      } catch {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        setToken(null);
        setActor(null);
      } finally {
        setIsLoading(false);
      }
    }

    void bootstrap();
  }, [token]);

  const login = useCallback(async (email: string, password: string) => {
    const response: LoginResponse = await loginWithPassword(email, password);
    localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
    setToken(response.accessToken);
    setActor(response.user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    setToken(null);
    setActor(null);
  }, []);

  const value = useMemo(
    () => ({
      token,
      actor,
      isLoading,
      login,
      logout,
    }),
    [actor, isLoading, login, logout, token],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}

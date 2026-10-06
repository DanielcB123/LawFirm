import { useState } from "react";
import type { FormEvent } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { ApiClientError } from "../services/apiClient";
import { useAuth } from "../features/auth/AuthContext";

export function WorkspaceLoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [email, setEmail] = useState("attorney@parkerreedlaw.com");
  const [password, setPassword] = useState("AttorneyPass!123");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await login(email, password);
      const redirectPath = (location.state as { from?: string } | undefined)?.from ?? "/workspace";
      navigate(redirectPath, { replace: true });
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Unable to sign in.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page">
      <h1>Workspace Sign In</h1>
      <p className="lead">Use a staff account to manage contacts and conflict checks.</p>

      <form className="card workspace-form" onSubmit={onSubmit}>
        <label htmlFor="email">Email</label>
        <input id="email" value={email} onChange={(event) => setEmail(event.target.value)} />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />

        {error && <p className="error-text">Sign-in failed: {error}</p>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Signing in..." : "Sign in"}
        </button>
      </form>
    </section>
  );
}

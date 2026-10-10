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
    <section className="page marketing-page">
      <section className="marketing-hero" data-reveal="zoom">
        <p className="eyebrow">Workspace access</p>
        <h1>Workspace Sign In</h1>
        <p className="lead">Use a staff account to manage contacts, calendars, matters, and intake workflows.</p>
      </section>

      <form className="card workspace-form" onSubmit={onSubmit} data-reveal="left">
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

      <section className="content-grid marketing-grid" data-reveal="right">
        <article className="card marketing-card" data-reveal>
          <h2>Who uses workspace</h2>
          <ul>
            <li>Attorneys managing matters and deadlines.</li>
            <li>Paralegals handling intake and document prep.</li>
            <li>Legal assistants coordinating appointments and communications.</li>
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="zoom">
          <h2>Need help signing in?</h2>
          <p>
            If you do not have access yet, contact your office administrator for account setup and role permissions.
          </p>
        </article>
      </section>
    </section>
  );
}

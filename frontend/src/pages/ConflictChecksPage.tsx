import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { runConflictCheck, submitConflictDecision } from "../services/conflictsApi";
import type { ConflictCheckResult } from "../types/parties";

const decisionOptions = ["Approved", "Rejected", "NeedsMoreInfo"] as const;

export function ConflictChecksPage() {
  const { token } = useAuth();
  const [query, setQuery] = useState("");
  const [result, setResult] = useState<ConflictCheckResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [decision, setDecision] = useState<(typeof decisionOptions)[number]>("NeedsMoreInfo");
  const [rationale, setRationale] = useState("");
  const [savedMessage, setSavedMessage] = useState<string | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [isSavingDecision, setIsSavingDecision] = useState(false);

  async function onRunCheck(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setIsChecking(true);
    setError(null);
    setSavedMessage(null);

    try {
      const check = await runConflictCheck(token, query);
      setResult(check);
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Conflict check failed.";
      setError(message);
    } finally {
      setIsChecking(false);
    }
  }

  async function onRecordDecision(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !result) {
      return;
    }

    setIsSavingDecision(true);
    setError(null);
    setSavedMessage(null);

    try {
      await submitConflictDecision(token, result.checkId, { decision, rationale });
      setSavedMessage("Conflict decision recorded with retained snapshot.");
      setRationale("");
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to save decision.";
      setError(message);
    } finally {
      setIsSavingDecision(false);
    }
  }

  return (
    <section className="page">
      <h1>Conflict Review</h1>
      <p className="lead">Run conflict checks, review match reasons, and record attorney decisions with rationale.</p>

      <section className="content-grid">
        <article className="card">
          <h2>Run check</h2>
          <form className="workspace-form" onSubmit={onRunCheck}>
            <label htmlFor="query">Prospect/client/adverse name</label>
            <input id="query" value={query} onChange={(event) => setQuery(event.target.value)} required />
            <button type="submit" disabled={isChecking}>
              {isChecking ? "Checking..." : "Run conflict check"}
            </button>
          </form>
          {error && <p className="error-text">{error}</p>}
        </article>

        <article className="card">
          <h2>Matches</h2>
          {!result && <p>No conflict check has been run yet.</p>}
          {result && (
            <>
              <p className="muted">
                Check ID: {result.checkId} · Correlation: {result.correlationId}
              </p>
              {result.matches.length === 0 ? (
                <p>No matches found.</p>
              ) : (
                <ul className="list-reset">
                  {result.matches.map((match) => (
                    <li key={match.partyId} className="party-row">
                      <p>
                        <strong>{match.displayName}</strong> ({match.partyType}) · score {match.score.toFixed(3)}
                      </p>
                      <p className="muted">{match.matchReason}</p>
                    </li>
                  ))}
                </ul>
              )}
            </>
          )}
        </article>
      </section>

      {result && (
        <article className="card">
          <h2>Record decision</h2>
          <form className="workspace-form" onSubmit={onRecordDecision}>
            <label htmlFor="decision">Decision</label>
            <select
              id="decision"
              value={decision}
              onChange={(event) => setDecision(event.target.value as (typeof decisionOptions)[number])}
            >
              {decisionOptions.map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </select>

            <label htmlFor="rationale">Rationale</label>
            <textarea
              id="rationale"
              value={rationale}
              onChange={(event) => setRationale(event.target.value)}
              rows={5}
              required
            />

            <button type="submit" disabled={isSavingDecision}>
              {isSavingDecision ? "Saving..." : "Save decision"}
            </button>
            {savedMessage && <p>{savedMessage}</p>}
          </form>
        </article>
      )}
    </section>
  );
}

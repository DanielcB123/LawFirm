import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { getMatterList } from "../services/mattersApi";
import type { MatterListItem } from "../types/matters";

export function MattersPage() {
  const { token } = useAuth();
  const [matters, setMatters] = useState<MatterListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      if (!token) {
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        const result = await getMatterList(token);
        setMatters(result);
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load matters.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [token]);

  return (
    <section className="page">
      <h1>Matters</h1>
      <p className="lead">Active matter shells converted from accepted intake records.</p>
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading matters...</p>}

      {!isLoading && (
        <article className="card">
          <h2>Matter list</h2>
          {matters.length === 0 && <p>No matters yet.</p>}
          <ul className="list-reset">
            {matters.map((matter) => (
              <li key={matter.id} className="party-row">
                <Link to={`/workspace/matters/${matter.id}`}>
                  <strong>{matter.matterNumber}</strong> — {matter.title}
                </Link>
                <p className="muted">
                  {matter.practiceArea} • Stage: {matter.stage} • Attorney: {matter.responsibleAttorneyDisplayName}
                </p>
              </li>
            ))}
          </ul>
        </article>
      )}
    </section>
  );
}

import { useEffect, useState } from "react";
import { NavLink, Outlet, useParams } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { getMatterById } from "../services/mattersApi";
import type { MatterDetail } from "../types/matters";

const tabs = [
  { label: "Overview", path: "overview" },
  { label: "Tasks & Deadlines", path: "tasks" },
  { label: "Documents", path: "documents" },
  { label: "Timeline", path: "timeline" },
];

export function MatterWorkspacePage() {
  const { matterId } = useParams();
  const { token } = useAuth();
  const [matter, setMatter] = useState<MatterDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      if (!token || !matterId) {
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        const result = await getMatterById(token, matterId);
        setMatter(result);
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load matter workspace.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [token, matterId]);

  if (isLoading) {
    return (
      <section className="page">
        <h1>Matter workspace</h1>
        <p>Loading matter...</p>
      </section>
    );
  }

  if (!matter) {
    return (
      <section className="page">
        <h1>Matter workspace</h1>
        <p className="error-text">{error ?? "Matter not found."}</p>
      </section>
    );
  }

  return (
    <section className="page">
      <h1>{matter.matterNumber}</h1>
      <p className="lead">
        {matter.title} • {matter.practiceArea} • Stage: {matter.stage} • Attorney:{" "}
        {matter.responsibleAttorneyDisplayName}
      </p>
      {error && <p className="error-text">{error}</p>}

      <article className="card">
        <div className="matter-tablist">
          {tabs.map((tab) => (
            <NavLink
              key={tab.path}
              to={tab.path}
              className={({ isActive }) => `matter-tab ${isActive ? "matter-tab--active" : ""}`}
            >
              {tab.label}
            </NavLink>
          ))}
        </div>

        <Outlet context={{ matter }} />
      </article>
    </section>
  );
}

import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../features/auth";
import { ApiClientError } from "../../lib/apiClient";
import { getMatterList, getMatterTimelineActions } from "../../features/matters";

type ActivityRow = {
  id: string;
  actionType: string;
  summary: string;
  occurredAtUtc: string;
  byline: string;
  matterPath: string;
};

export function ActivitiesPage() {
  const { token } = useAuth();
  const [rows, setRows] = useState<ActivityRow[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function load() {
      if (!token) {
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const matters = await getMatterList(token);
        const timelines = await Promise.all(
          matters.map(async (matter) => {
            const actions = await getMatterTimelineActions(token, matter.id);
            return actions.map(
              (action): ActivityRow => ({
                id: action.id,
                actionType: action.actionType,
                summary: action.summary,
                occurredAtUtc: action.occurredAtUtc,
                byline: `${matter.matterNumber} · ${action.createdByDisplayName}`,
                matterPath: `/workspace/matters/${matter.id}/timeline`,
              }),
            );
          }),
        );

        setRows(
          timelines
            .flat()
            .sort((left, right) => new Date(right.occurredAtUtc).getTime() - new Date(left.occurredAtUtc).getTime()),
        );
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load activities.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [token]);

  return (
    <section className="page">
      <h1>Activities</h1>
      <p className="lead">Work recorded on the matters you can open, newest first.</p>
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading activities...</p>}
      {!isLoading && !error && rows.length === 0 && <p>No activity has been recorded on your matters.</p>}
      {!isLoading && rows.length > 0 && (
        <article className="card">
          <p className="muted">
            {rows.length} {rows.length === 1 ? "activity" : "activities"}
          </p>
          <ul className="list-reset">
            {rows.map((row) => (
              <li key={row.id} className="contact-row">
                <div className="contact-row__main">
                  <Link to={row.matterPath}>
                    <strong>
                      {row.actionType}: {row.summary}
                    </strong>
                  </Link>
                </div>
                <p className="muted">{row.byline}</p>
                <p className="muted">{new Date(row.occurredAtUtc).toLocaleString()}</p>
              </li>
            ))}
          </ul>
        </article>
      )}
    </section>
  );
}

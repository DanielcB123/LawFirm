import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { getCalendarEntries } from "../services/calendarApi";
import { getMatterList } from "../services/mattersApi";
import type { CalendarEntry } from "../types/calendar";
import type { MatterListItem } from "../types/matters";

type WorklistView = "open-tasks" | "due-today" | "overdue" | "active-matters";

const viewLabels: Record<WorklistView, string> = {
  "open-tasks": "Open tasks",
  "due-today": "Due today",
  overdue: "Overdue",
  "active-matters": "Active matters",
};

function toDayStartUtc(date: Date): number {
  return Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
}

export function WorkspaceWorklistsPage() {
  const { token, actor } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const currentView = (searchParams.get("view") ?? "open-tasks") as WorklistView;
  const validView: WorklistView = ["open-tasks", "due-today", "overdue", "active-matters"].includes(currentView)
    ? currentView
    : "open-tasks";

  const permissions = actor?.permissions ?? [];
  const canReadCalendar = permissions.includes("calendar.read");
  const canReadMatters = permissions.includes("matters.read.assigned") || permissions.includes("matters.read.all");

  const [calendarEntries, setCalendarEntries] = useState<CalendarEntry[]>([]);
  const [matters, setMatters] = useState<MatterListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      if (!token) {
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        const [calendarResult, mattersResult] = await Promise.all([
          canReadCalendar ? getCalendarEntries(token) : Promise.resolve<CalendarEntry[]>([]),
          canReadMatters ? getMatterList(token) : Promise.resolve<MatterListItem[]>([]),
        ]);
        setCalendarEntries(calendarResult);
        setMatters(mattersResult);
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load worklists.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [canReadCalendar, canReadMatters, token]);

  const now = new Date();
  const todayUtcStart = toDayStartUtc(now);
  const tomorrowUtcStart = todayUtcStart + 24 * 60 * 60 * 1000;

  const openTasks = useMemo(
    () => calendarEntries.filter((entry) => !entry.isCompleted && entry.entryType !== "Event"),
    [calendarEntries],
  );

  const dueTodayTasks = useMemo(
    () =>
      openTasks.filter((entry) => {
        const scheduledAt = new Date(entry.scheduledAtUtc).getTime();
        return scheduledAt >= todayUtcStart && scheduledAt < tomorrowUtcStart;
      }),
    [openTasks, todayUtcStart, tomorrowUtcStart],
  );

  const overdueTasks = useMemo(
    () => openTasks.filter((entry) => new Date(entry.scheduledAtUtc).getTime() < now.getTime()),
    [openTasks, now],
  );

  const activeMatters = useMemo(() => matters.filter((matter) => matter.status === "Active"), [matters]);

  return (
    <section className="page">
      <h1>Worklists</h1>
      <p className="lead">Jump directly into prioritized task and matter queues from your operations dashboard metrics.</p>

      <div className="worklist-tabs" role="tablist" aria-label="Worklist filters">
        {(Object.keys(viewLabels) as WorklistView[]).map((view) => (
          <button
            key={view}
            type="button"
            className={`calendar-tab ${validView === view ? "calendar-tab--active" : ""}`}
            role="tab"
            aria-selected={validView === view}
            onClick={() => setSearchParams({ view })}
          >
            {viewLabels[view]}
          </button>
        ))}
      </div>

      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading worklists...</p>}

      {!isLoading && validView !== "active-matters" && !canReadCalendar && (
        <p className="error-text">You do not have permission to view calendar task worklists.</p>
      )}
      {!isLoading && validView === "active-matters" && !canReadMatters && (
        <p className="error-text">You do not have permission to view matter worklists.</p>
      )}

      {!isLoading && canReadCalendar && validView === "open-tasks" && (
        <article className="card">
          <h2>Open tasks ({openTasks.length})</h2>
          {openTasks.length === 0 ? (
            <p className="muted">No open tasks.</p>
          ) : (
            <table className="workspace-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Type</th>
                  <th>Scheduled</th>
                  <th>Owner</th>
                </tr>
              </thead>
              <tbody>
                {openTasks.map((entry) => (
                  <tr key={entry.id}>
                    <td>{entry.title}</td>
                    <td>{entry.entryType}</td>
                    <td>{new Date(entry.scheduledAtUtc).toLocaleString()}</td>
                    <td>{entry.ownerDisplayName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Link className="mini-action-link" to="/workspace/calendar?view=month">
            Open calendar workspace
          </Link>
        </article>
      )}

      {!isLoading && canReadCalendar && validView === "due-today" && (
        <article className="card">
          <h2>Due today ({dueTodayTasks.length})</h2>
          {dueTodayTasks.length === 0 ? (
            <p className="muted">No tasks due today.</p>
          ) : (
            <table className="workspace-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Type</th>
                  <th>Time</th>
                  <th>Owner</th>
                </tr>
              </thead>
              <tbody>
                {dueTodayTasks.map((entry) => (
                  <tr key={entry.id}>
                    <td>{entry.title}</td>
                    <td>{entry.entryType}</td>
                    <td>{new Date(entry.scheduledAtUtc).toLocaleTimeString([], { hour: "numeric", minute: "2-digit" })}</td>
                    <td>{entry.ownerDisplayName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Link className="mini-action-link" to="/workspace/calendar?view=month">
            Open calendar workspace
          </Link>
        </article>
      )}

      {!isLoading && canReadCalendar && validView === "overdue" && (
        <article className="card">
          <h2>Overdue ({overdueTasks.length})</h2>
          {overdueTasks.length === 0 ? (
            <p className="muted">No overdue tasks.</p>
          ) : (
            <table className="workspace-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Type</th>
                  <th>Scheduled</th>
                  <th>Owner</th>
                </tr>
              </thead>
              <tbody>
                {overdueTasks.map((entry) => (
                  <tr key={entry.id}>
                    <td>{entry.title}</td>
                    <td>{entry.entryType}</td>
                    <td>{new Date(entry.scheduledAtUtc).toLocaleString()}</td>
                    <td>{entry.ownerDisplayName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Link className="mini-action-link" to="/workspace/calendar?view=month">
            Open calendar workspace
          </Link>
        </article>
      )}

      {!isLoading && canReadMatters && validView === "active-matters" && (
        <article className="card">
          <h2>Active matters ({activeMatters.length})</h2>
          {activeMatters.length === 0 ? (
            <p className="muted">No active matters.</p>
          ) : (
            <table className="workspace-table">
              <thead>
                <tr>
                  <th>Matter</th>
                  <th>Title</th>
                  <th>Stage</th>
                  <th>Attorney</th>
                </tr>
              </thead>
              <tbody>
                {activeMatters.map((matter) => (
                  <tr key={matter.id}>
                    <td>
                      <Link to={`/workspace/matters/${matter.id}/overview`}>{matter.matterNumber}</Link>
                    </td>
                    <td>{matter.title}</td>
                    <td>{matter.stage}</td>
                    <td>{matter.responsibleAttorneyDisplayName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Link className="mini-action-link" to="/workspace/matters">
            Open matters workspace
          </Link>
        </article>
      )}
    </section>
  );
}

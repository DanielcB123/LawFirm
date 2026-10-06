import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { getCalendarEntries } from "../services/calendarApi";
import { ApiClientError } from "../services/apiClient";
import { getIntakeList } from "../services/intakeApi";
import { getMatterList, getMatterTimelineActions } from "../services/mattersApi";
import type { CalendarEntry } from "../types/calendar";
import type { IntakeListItem } from "../types/intake";
import type { MatterListItem } from "../types/matters";

type ActivityItem = {
  id: string;
  label: string;
  occurredAtUtc: string;
  linkPath: string;
  byline: string;
};

function toDayStartUtc(date: Date): number {
  return Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
}

export function WorkspaceHomePage() {
  const { token, actor } = useAuth();
  const permissions = actor?.permissions ?? [];
  const permissionSet = useMemo(() => new Set(permissions), [permissions]);

  const canReadCalendar = permissionSet.has("calendar.read");
  const canReadIntake = permissionSet.has("intake.read");
  const canReadMatters = permissionSet.has("matters.read.assigned") || permissionSet.has("matters.read.all");

  const [calendarEntries, setCalendarEntries] = useState<CalendarEntry[]>([]);
  const [intakeItems, setIntakeItems] = useState<IntakeListItem[]>([]);
  const [matters, setMatters] = useState<MatterListItem[]>([]);
  const [timelineItems, setTimelineItems] = useState<ActivityItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadDashboard() {
      if (!token || !actor) {
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        const [calendarResult, intakeResult, mattersResult] = await Promise.all([
          canReadCalendar ? getCalendarEntries(token) : Promise.resolve<CalendarEntry[]>([]),
          canReadIntake ? getIntakeList(token) : Promise.resolve<IntakeListItem[]>([]),
          canReadMatters ? getMatterList(token) : Promise.resolve<MatterListItem[]>([]),
        ]);

        setCalendarEntries(calendarResult);
        setIntakeItems(intakeResult);
        setMatters(mattersResult);

        if (canReadMatters && mattersResult.length > 0) {
          const timelineSources = mattersResult.slice(0, 4);
          const timelineResults = await Promise.all(
            timelineSources.map(async (matter) => {
              try {
                const actions = await getMatterTimelineActions(token, matter.id);
                return actions.map((action) => ({
                  id: action.id,
                  action,
                  matter,
                }));
              } catch {
                return [];
              }
            }),
          );

          const mergedTimeline = timelineResults
            .flat()
            .sort(
              (left, right) =>
                new Date(right.action.occurredAtUtc).getTime() - new Date(left.action.occurredAtUtc).getTime(),
            )
            .slice(0, 6)
            .map(({ action, matter }): ActivityItem => ({
              id: action.id,
              label: `${action.actionType}: ${action.summary}`,
              occurredAtUtc: action.occurredAtUtc,
              linkPath: `/workspace/matters/${matter.id}/timeline`,
              byline: `${matter.matterNumber} • ${action.createdByDisplayName}`,
            }));

          setTimelineItems(mergedTimeline);
        } else {
          setTimelineItems([]);
        }
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load workspace dashboard.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadDashboard();
  }, [actor, canReadCalendar, canReadIntake, canReadMatters, token]);

  const now = new Date();
  const todayUtcStart = toDayStartUtc(now);
  const tomorrowUtcStart = todayUtcStart + 24 * 60 * 60 * 1000;

  const assignedOpenTasks = useMemo(
    () => calendarEntries.filter((entry) => !entry.isCompleted && entry.entryType !== "Event").length,
    [calendarEntries],
  );

  const overdueTasks = useMemo(
    () => calendarEntries.filter((entry) => !entry.isCompleted && new Date(entry.scheduledAtUtc).getTime() < now.getTime()).length,
    [calendarEntries, now],
  );

  const dueTodayCount = useMemo(
    () =>
      calendarEntries.filter((entry) => {
        if (entry.isCompleted) {
          return false;
        }

        const scheduledAt = new Date(entry.scheduledAtUtc).getTime();
        return scheduledAt >= todayUtcStart && scheduledAt < tomorrowUtcStart;
      }).length,
    [calendarEntries, todayUtcStart, tomorrowUtcStart],
  );

  const intakeByStage = useMemo(() => {
    const counts = new Map<string, number>();
    for (const item of intakeItems) {
      counts.set(item.stage, (counts.get(item.stage) ?? 0) + 1);
    }
    return counts;
  }, [intakeItems]);

  const activeMatterCount = useMemo(() => matters.filter((matter) => matter.status === "Active").length, [matters]);

  const atRiskMatterCount = useMemo(
    () => matters.filter((matter) => matter.stage === "Escalated" || matter.stage === "Blocked").length,
    [matters],
  );

  const quickActions = useMemo(
    () =>
      [
        canReadCalendar
          ? { label: "Plan task on calendar", path: "/workspace/calendar", helper: "Schedule a new deadline or follow-up." }
          : null,
        canReadIntake
          ? { label: "Review intake board", path: "/workspace/intake", helper: "Move prospects through intake stages." }
          : null,
        canReadIntake
          ? {
              label: "Process consultation queue",
              path: "/workspace/consultations",
              helper: "Triage incoming contact requests and follow-up ownership.",
            }
          : null,
        canReadMatters
          ? { label: "Open matters workspace", path: "/workspace/matters", helper: "Drill into tasks, docs, and timeline." }
          : null,
        permissionSet.has("conflicts.search")
          ? { label: "Run conflict check", path: "/workspace/conflicts", helper: "Validate potential adverse parties." }
          : null,
      ].filter((item): item is { label: string; path: string; helper: string } => item !== null),
    [canReadCalendar, canReadIntake, canReadMatters, permissionSet],
  );

  return (
    <section className="page">
      <h1>Operations Cockpit</h1>
      <p className="lead">
        Role-aware workspace for {actor?.displayName}. Monitor deadlines, intake, matter load, and recent activity in one view.
      </p>
      {error && <p className="error-text">{error}</p>}

      <section className="dashboard-metrics">
        <article className="card metric-card">
          <p className="metric-card__label">Open tasks</p>
          <p className="metric-card__value">{canReadCalendar ? assignedOpenTasks : "N/A"}</p>
          <p className="muted">Calendar tasks and deadlines not marked complete.</p>
        </article>
        <article className="card metric-card">
          <p className="metric-card__label">Due today</p>
          <p className="metric-card__value">{canReadCalendar ? dueTodayCount : "N/A"}</p>
          <p className="muted">Entries scheduled before end of day.</p>
        </article>
        <article className="card metric-card">
          <p className="metric-card__label">Overdue</p>
          <p className="metric-card__value">{canReadCalendar ? overdueTasks : "N/A"}</p>
          <p className="muted">Unfinished entries with schedule time in the past.</p>
        </article>
        <article className="card metric-card">
          <p className="metric-card__label">Active matters</p>
          <p className="metric-card__value">{canReadMatters ? activeMatterCount : "N/A"}</p>
          <p className="muted">Current matters requiring legal team execution.</p>
        </article>
      </section>

      {isLoading && <p>Loading dashboard...</p>}

      {!isLoading && (
        <section className="dashboard-grid">
          <article className="card">
            <h2>Quick actions</h2>
            {quickActions.length === 0 && <p className="muted">No actions available for current permissions.</p>}
            <div className="dashboard-actions">
              {quickActions.map((action) => (
                <Link key={action.path} to={action.path} className="dashboard-action">
                  <strong>{action.label}</strong>
                  <span className="muted">{action.helper}</span>
                </Link>
              ))}
            </div>
          </article>

          <article className="card">
            <h2>Intake pipeline snapshot</h2>
            {!canReadIntake && <p className="muted">You do not have intake permissions.</p>}
            {canReadIntake && intakeItems.length === 0 && <p className="muted">No intake records yet.</p>}
            {canReadIntake && intakeItems.length > 0 && (
              <ul className="list-reset">
                {["New", "Contacted", "ConsultationScheduled", "UnderReview", "Accepted", "Declined", "ReferredElsewhere"].map(
                  (stage) => (
                    <li key={stage} className="party-row">
                      <p>
                        <strong>{stage}</strong>: {intakeByStage.get(stage) ?? 0}
                      </p>
                    </li>
                  ),
                )}
              </ul>
            )}
            {canReadIntake && (
              <Link to="/workspace/intake" className="mini-action-link">
                Open intake board
              </Link>
            )}
          </article>

          <article className="card">
            <h2>Matter load</h2>
            {!canReadMatters && <p className="muted">You do not have matter read permissions.</p>}
            {canReadMatters && (
              <>
                <p>
                  <strong>Total matters:</strong> {matters.length}
                </p>
                <p>
                  <strong>Active:</strong> {activeMatterCount}
                </p>
                <p>
                  <strong>At risk:</strong> {atRiskMatterCount}
                </p>
                <ul className="list-reset">
                  {matters.slice(0, 5).map((matter) => (
                    <li key={matter.id} className="party-row">
                      <Link to={`/workspace/matters/${matter.id}/overview`}>
                        <strong>{matter.matterNumber}</strong> - {matter.title}
                      </Link>
                      <p className="muted">
                        {matter.stage} • {matter.responsibleAttorneyDisplayName}
                      </p>
                    </li>
                  ))}
                </ul>
              </>
            )}
          </article>

          <article className="card">
            <h2>Recent timeline activity</h2>
            {!canReadMatters && <p className="muted">You do not have matter activity permissions.</p>}
            {canReadMatters && timelineItems.length === 0 && <p className="muted">No recent timeline actions.</p>}
            {canReadMatters && timelineItems.length > 0 && (
              <ul className="list-reset">
                {timelineItems.map((item) => (
                  <li key={item.id} className="party-row">
                    <Link to={item.linkPath}>
                      <strong>{item.label}</strong>
                    </Link>
                    <p className="muted">{item.byline}</p>
                    <p className="muted">{new Date(item.occurredAtUtc).toLocaleString()}</p>
                  </li>
                ))}
              </ul>
            )}
          </article>
        </section>
      )}
    </section>
  );
}

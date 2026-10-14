import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../features/auth";
import { getCalendarEntries } from "../../features/calendar";
import { getIntakeList } from "../../features/intake";
import { ApiClientError } from "../../lib/apiClient";
import { getMatterList } from "../../features/matters";
import { searchParties } from "../../features/parties";

type ReportCard = {
  label: string;
  value: string;
  detail: string;
  path: string;
};

export function ReportsPage() {
  const { token, actor } = useAuth();
  const [cards, setCards] = useState<ReportCard[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function load() {
      if (!token || !actor) {
        return;
      }

      const permissions = new Set(actor.permissions);
      setIsLoading(true);
      setError(null);

      try {
        const nextCards: ReportCard[] = [];

        if (permissions.has("calendar.read")) {
          const entries = await getCalendarEntries(token);
          const openTasks = entries.filter((entry) => !entry.isCompleted && entry.entryType !== "Event").length;
          nextCards.push({
            label: "Open tasks",
            value: String(openTasks),
            detail: "Tasks and deadlines that are not complete.",
            path: "/workspace/worklists",
          });
        }

        if (permissions.has("matters.read.assigned") || permissions.has("matters.read.all")) {
          const matters = await getMatterList(token);
          const active = matters.filter((matter) => matter.status === "Active").length;
          nextCards.push({
            label: "Active matters",
            value: String(active),
            detail: `${matters.length} ${matters.length === 1 ? "matter" : "matters"} you can open.`,
            path: "/workspace/matters",
          });
        }

        if (permissions.has("intake.read")) {
          const intake = await getIntakeList(token);
          nextCards.push({
            label: "Intake records",
            value: String(intake.length),
            detail: "Prospects currently on the intake board.",
            path: "/workspace/intake",
          });
        }

        if (permissions.has("contacts.read")) {
          const contacts = await searchParties(token, "");
          nextCards.push({
            label: "Contacts",
            value: String(contacts.length),
            detail: "People and organizations in the directory.",
            path: "/workspace/contacts",
          });
        }

        setCards(nextCards);
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load reports.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [actor, token]);

  return (
    <section className="page">
      <h1>Reports</h1>
      <p className="lead">Counts from the records already in the workspace. These are not financial reports.</p>
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading reports...</p>}
      {!isLoading && !error && cards.length === 0 && <p>No reports are available for your permissions.</p>}
      {!isLoading && cards.length > 0 && (
        <section className="dashboard-metrics">
          {cards.map((card) => (
            <Link key={card.path} to={card.path} className="card metric-card metric-card--link">
              <p className="metric-card__label">{card.label}</p>
              <p className="metric-card__value">{card.value}</p>
              <p className="muted">{card.detail}</p>
            </Link>
          ))}
        </section>
      )}
    </section>
  );
}

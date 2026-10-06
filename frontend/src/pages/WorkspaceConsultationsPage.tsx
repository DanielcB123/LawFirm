import { useEffect, useMemo, useState } from "react";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { getConsultationQueue, updateConsultationStatus } from "../services/consultationsApi";
import type { ConsultationRequestItem } from "../types/consultations";

const statuses = ["New", "InReview", "Contacted", "Scheduled", "Closed", "Cancelled"];

type DraftState = {
  status: string;
  internalNotes: string;
};

export function WorkspaceConsultationsPage() {
  const { token, actor } = useAuth();
  const canReadConsultations = useMemo(() => (actor?.permissions ?? []).includes("intake.read"), [actor?.permissions]);
  const canManageConsultations = useMemo(() => (actor?.permissions ?? []).includes("intake.manage"), [actor?.permissions]);

  const [items, setItems] = useState<ConsultationRequestItem[]>([]);
  const [drafts, setDrafts] = useState<Record<string, DraftState>>({});
  const [isLoading, setIsLoading] = useState(true);
  const [isSavingId, setIsSavingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadQueue() {
      if (!token || !canReadConsultations) {
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        const queue = await getConsultationQueue(token);
        setItems(queue);
        setDrafts(
          Object.fromEntries(
            queue.map((item) => [
              item.id,
              {
                status: item.status,
                internalNotes: item.internalNotes ?? "",
              },
            ]),
          ),
        );
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load consultation queue.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadQueue();
  }, [canReadConsultations, token]);

  async function saveItem(item: ConsultationRequestItem) {
    if (!token || !canManageConsultations) {
      return;
    }

    const draft = drafts[item.id];
    if (!draft) {
      return;
    }

    setIsSavingId(item.id);
    setError(null);
    try {
      const updated = await updateConsultationStatus(token, item.id, {
        status: draft.status,
        internalNotes: draft.internalNotes || undefined,
        assignedToActorId: actor?.id,
        assignedToDisplayName: actor?.displayName,
      });
      setItems((current) => current.map((existing) => (existing.id === updated.id ? updated : existing)));
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to update consultation.");
    } finally {
      setIsSavingId(null);
    }
  }

  return (
    <section className="page">
      <h1>Consultation Queue</h1>
      <p className="lead">
        Review incoming consultation requests, update their status, and assign follow-up so paralegals and attorneys
        can start client onboarding quickly.
      </p>

      {!canReadConsultations && <p className="error-text">You do not have permission to view consultation requests.</p>}
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading consultation queue...</p>}
      {!isLoading && canReadConsultations && items.length === 0 && <p className="muted">No consultation requests yet.</p>}

      {!isLoading && canReadConsultations && items.length > 0 && (
        <section className="consultation-queue">
          {items.map((item) => {
            const draft = drafts[item.id] ?? { status: item.status, internalNotes: item.internalNotes ?? "" };
            return (
              <article key={item.id} className="card consultation-item">
                <div className="consultation-item__header">
                  <h2>{item.fullName}</h2>
                  <span className={`consultation-status consultation-status--${item.status.toLowerCase()}`}>{item.status}</span>
                </div>
                <p>
                  <strong>Email:</strong> {item.email}
                </p>
                <p>
                  <strong>Phone:</strong> {item.phone || "Not provided"}
                </p>
                <p>
                  <strong>Practice area:</strong> {item.practiceArea}
                </p>
                <p>
                  <strong>Requested appointment:</strong> {new Date(item.preferredAtUtc).toLocaleString()} ({item.timeZone})
                </p>
                <p>
                  <strong>Message:</strong> {item.message || "No message provided."}
                </p>
                <p className="muted">Submitted {new Date(item.createdAtUtc).toLocaleString()}</p>

                <div className="consultation-item__controls">
                  <label htmlFor={`status-${item.id}`}>Status</label>
                  <select
                    id={`status-${item.id}`}
                    value={draft.status}
                    onChange={(event) =>
                      setDrafts((current) => ({
                        ...current,
                        [item.id]: {
                          ...draft,
                          status: event.target.value,
                        },
                      }))
                    }
                    disabled={!canManageConsultations || isSavingId === item.id}
                  >
                    {statuses.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>

                  <label htmlFor={`notes-${item.id}`}>Internal notes</label>
                  <textarea
                    id={`notes-${item.id}`}
                    rows={3}
                    value={draft.internalNotes}
                    onChange={(event) =>
                      setDrafts((current) => ({
                        ...current,
                        [item.id]: {
                          ...draft,
                          internalNotes: event.target.value,
                        },
                      }))
                    }
                    disabled={!canManageConsultations || isSavingId === item.id}
                  />

                  <button
                    type="button"
                    onClick={() => void saveItem(item)}
                    disabled={!canManageConsultations || isSavingId === item.id}
                  >
                    {isSavingId === item.id ? "Saving..." : "Save update"}
                  </button>
                </div>
              </article>
            );
          })}
        </section>
      )}
    </section>
  );
}

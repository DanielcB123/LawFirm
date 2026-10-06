import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { createIntake, getIntakeList } from "../services/intakeApi";
import type { IntakeListItem } from "../types/intake";
import type { FormEvent } from "react";

const stageOrder = [
  "New",
  "Contacted",
  "ConsultationScheduled",
  "UnderReview",
  "Accepted",
  "Declined",
  "ReferredElsewhere",
] as const;

export function IntakeBoardPage() {
  const { token, actor } = useAuth();
  const [items, setItems] = useState<IntakeListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [clientName, setClientName] = useState("");
  const [practiceArea, setPracticeArea] = useState("PersonalInjury");
  const [intakeType, setIntakeType] = useState("StaffEntered");
  const [nextAction, setNextAction] = useState("");
  const [notesSummary, setNotesSummary] = useState("");

  async function loadBoard() {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const result = await getIntakeList(token);
      setItems(result);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to load intake board.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadBoard();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const grouped = useMemo(() => {
    const map = new Map<string, IntakeListItem[]>();
    for (const stage of stageOrder) {
      map.set(stage, []);
    }
    for (const item of items) {
      if (!map.has(item.stage)) {
        map.set(item.stage, []);
      }
      map.get(item.stage)?.push(item);
    }
    return map;
  }, [items]);

  async function onCreateIntake(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !actor) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await createIntake(token, {
        prospectiveClientName: clientName,
        practiceArea,
        intakeType,
        ownerActorId: actor.id,
        ownerDisplayName: actor.displayName,
        nextAction: nextAction || undefined,
        notesSummary: notesSummary || undefined,
      });
      setClientName("");
      setNextAction("");
      setNotesSummary("");
      await loadBoard();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to create intake.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page">
      <h1>Intake Pipeline</h1>
      <p className="lead">
        Track prospective matters through review stages. Accepted intake records can be converted into matter shells.
      </p>

      <article className="card">
        <h2>Create intake</h2>
        <form className="workspace-form" onSubmit={onCreateIntake}>
          <label htmlFor="intakeClientName">Prospective client name</label>
          <input
            id="intakeClientName"
            value={clientName}
            onChange={(event) => setClientName(event.target.value)}
            required
          />

          <label htmlFor="intakePracticeArea">Practice area</label>
          <select
            id="intakePracticeArea"
            value={practiceArea}
            onChange={(event) => setPracticeArea(event.target.value)}
          >
            <option value="PersonalInjury">Personal Injury</option>
            <option value="ConstructionLitigation">Construction Litigation</option>
          </select>

          <label htmlFor="intakeType">Intake type</label>
          <select id="intakeType" value={intakeType} onChange={(event) => setIntakeType(event.target.value)}>
            <option value="StaffEntered">Staff Entered</option>
            <option value="Website">Website</option>
            <option value="Phone">Phone</option>
            <option value="Referral">Referral</option>
          </select>

          <label htmlFor="nextAction">Next action</label>
          <input id="nextAction" value={nextAction} onChange={(event) => setNextAction(event.target.value)} />

          <label htmlFor="notesSummary">Summary note</label>
          <textarea
            id="notesSummary"
            rows={3}
            value={notesSummary}
            onChange={(event) => setNotesSummary(event.target.value)}
          />

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create intake"}
          </button>
        </form>
      </article>

      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading intake board...</p>}

      {!isLoading && (
        <section className="kanban-board">
          {stageOrder.map((stage) => (
            <article key={stage} className="kanban-column">
              <h3>
                {stage} ({grouped.get(stage)?.length ?? 0})
              </h3>
              <div className="kanban-cards">
                {(grouped.get(stage) ?? []).map((item) => (
                  <Link key={item.id} to={`/workspace/intake/${item.id}`} className="kanban-card">
                    <p>
                      <strong>{item.prospectiveClientName}</strong>
                    </p>
                    <p className="muted">{item.practiceArea}</p>
                    <p className="muted">Owner: {item.ownerDisplayName}</p>
                    {item.nextAction && <p className="muted">Next: {item.nextAction}</p>}
                  </Link>
                ))}
              </div>
            </article>
          ))}
        </section>
      )}
    </section>
  );
}

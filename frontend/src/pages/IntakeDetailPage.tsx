import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { addIntakeNote, changeIntakeStage, convertIntakeToMatter, getIntakeById } from "../services/intakeApi";
import type { IntakeDetail } from "../types/intake";
import type { FormEvent } from "react";

const targetStages = [
  "Contacted",
  "ConsultationScheduled",
  "UnderReview",
  "Accepted",
  "Declined",
  "ReferredElsewhere",
];

export function IntakeDetailPage() {
  const { intakeId } = useParams();
  const { token, actor } = useAuth();
  const navigate = useNavigate();

  const [detail, setDetail] = useState<IntakeDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [toStage, setToStage] = useState("UnderReview");
  const [stageReason, setStageReason] = useState("");
  const [nextAction, setNextAction] = useState("");
  const [declineReason, setDeclineReason] = useState("");
  const [approvedDecisionId, setApprovedDecisionId] = useState("");
  const [newNote, setNewNote] = useState("");
  const [matterTitle, setMatterTitle] = useState("");

  async function loadDetail() {
    if (!token || !intakeId) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const result = await getIntakeById(token, intakeId);
      setDetail(result);
      setNextAction(result.nextAction ?? "");
      setMatterTitle(`${result.prospectiveClientName} Matter`);
      setApprovedDecisionId(result.approvedConflictDecisionId ?? "");
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to load intake details.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadDetail();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, intakeId]);

  async function onChangeStage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !intakeId) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await changeIntakeStage(token, intakeId, {
        toStage,
        changeReason: stageReason || undefined,
        nextAction: nextAction || undefined,
        declineReason: declineReason || undefined,
        approvedConflictDecisionId: approvedDecisionId || undefined,
      });
      setStageReason("");
      await loadDetail();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to change stage.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onAddNote(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !intakeId || !newNote.trim()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await addIntakeNote(token, intakeId, newNote.trim());
      setNewNote("");
      await loadDetail();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to add note.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onConvertToMatter() {
    if (!token || !intakeId || !actor) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      const response = await convertIntakeToMatter(token, intakeId, {
        matterTitle,
        responsibleAttorneyActorId: actor.id,
        responsibleAttorneyDisplayName: actor.displayName,
      });
      navigate(`/workspace/matters/${response.matterId}`);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to convert intake to matter.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) {
    return (
      <section className="page">
        <h1>Intake Details</h1>
        <p>Loading intake details...</p>
      </section>
    );
  }

  if (!detail) {
    return (
      <section className="page">
        <h1>Intake Details</h1>
        <p className="error-text">Intake record not found.</p>
      </section>
    );
  }

  return (
    <section className="page">
      <h1>{detail.prospectiveClientName}</h1>
      <p className="lead">
        Stage: <strong>{detail.stage}</strong> • Practice area: <strong>{detail.practiceArea}</strong>
      </p>
      {error && <p className="error-text">{error}</p>}

      <section className="content-grid">
        <article className="card">
          <h2>Update stage</h2>
          <form className="workspace-form" onSubmit={onChangeStage}>
            <label htmlFor="toStage">Target stage</label>
            <select id="toStage" value={toStage} onChange={(event) => setToStage(event.target.value)}>
              {targetStages.map((stage) => (
                <option key={stage} value={stage}>
                  {stage}
                </option>
              ))}
            </select>

            <label htmlFor="nextAction">Next action</label>
            <input id="nextAction" value={nextAction} onChange={(event) => setNextAction(event.target.value)} />

            <label htmlFor="stageReason">Transition reason</label>
            <textarea
              id="stageReason"
              rows={3}
              value={stageReason}
              onChange={(event) => setStageReason(event.target.value)}
            />

            <label htmlFor="declineReason">Decline reason (if declining)</label>
            <input
              id="declineReason"
              value={declineReason}
              onChange={(event) => setDeclineReason(event.target.value)}
            />

            <label htmlFor="approvedDecisionId">Approved conflict decision ID (required for Accepted)</label>
            <input
              id="approvedDecisionId"
              value={approvedDecisionId}
              onChange={(event) => setApprovedDecisionId(event.target.value)}
            />

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Save stage update"}
            </button>
          </form>
        </article>

        <article className="card">
          <h2>Convert to matter</h2>
          <p className="muted">Conversion is allowed only when stage is Accepted.</p>
          <label htmlFor="matterTitle">Matter title</label>
          <input
            id="matterTitle"
            value={matterTitle}
            onChange={(event) => setMatterTitle(event.target.value)}
            className="workspace-inline-input"
          />
          <button
            type="button"
            className="mini-action"
            disabled={isSubmitting || detail.stage !== "Accepted"}
            onClick={onConvertToMatter}
          >
            Convert to matter
          </button>
        </article>
      </section>

      <section className="content-grid">
        <article className="card">
          <h2>Notes</h2>
          <form className="workspace-form" onSubmit={onAddNote}>
            <textarea rows={3} value={newNote} onChange={(event) => setNewNote(event.target.value)} />
            <button type="submit" disabled={isSubmitting || !newNote.trim()}>
              Add note
            </button>
          </form>

          <ul className="list-reset">
            {detail.notes.map((note) => (
              <li key={note.id} className="party-row">
                <p>{note.note}</p>
                <p className="muted">
                  {note.createdByDisplayName} • {new Date(note.createdAtUtc).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        </article>

        <article className="card">
          <h2>Stage history</h2>
          <ul className="list-reset">
            {detail.stageHistory.map((history) => (
              <li key={history.id} className="party-row">
                <p>
                  {history.fromStage} → {history.toStage}
                </p>
                <p className="muted">
                  {history.changedByDisplayName} • {new Date(history.changedAtUtc).toLocaleString()}
                </p>
                {history.changeReason && <p className="muted">{history.changeReason}</p>}
              </li>
            ))}
          </ul>
        </article>
      </section>
    </section>
  );
}

import { useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import {
  createMatterDocument,
  createMatterTask,
  createMatterTimelineAction,
  deleteMatterDocument,
  deleteMatterTask,
  getMatterDocuments,
  getMatterTasks,
  getMatterTimelineActions,
  reviewMatterDocument,
  updateMatterTask,
} from "../services/mattersApi";
import type { MatterDetail, MatterDocument, MatterTask, MatterTimelineAction } from "../types/matters";
import type { FormEvent } from "react";

type Context = { matter: MatterDetail };

function useMatterContext() {
  return useOutletContext<Context>();
}

function fromLocalInput(value: string): string | undefined {
  if (!value) {
    return undefined;
  }

  return new Date(value).toISOString();
}

export function MatterOverviewTab() {
  const { matter } = useMatterContext();

  return (
    <div className="matter-tab-panel">
      <h2>Overview</h2>
      <p>
        <strong>Status:</strong> {matter.status}
      </p>
      <p>
        <strong>Responsible attorney:</strong> {matter.responsibleAttorneyDisplayName}
      </p>
      <p>
        <strong>Paralegal:</strong> {matter.paralegalDisplayName ?? "Unassigned"}
      </p>
      <p>
        <strong>Linked intake:</strong> {matter.linkedIntakeRecordId ?? "Not linked"}
      </p>
    </div>
  );
}

export function MatterTasksTab() {
  const { matter } = useMatterContext();
  const { token } = useAuth();
  const [tasks, setTasks] = useState<MatterTask[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [dueAtLocal, setDueAtLocal] = useState("");
  const [assigneeDisplayName, setAssigneeDisplayName] = useState("");
  const [priority, setPriority] = useState("Normal");
  const [isDeadlineVerified, setIsDeadlineVerified] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadTasks() {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const result = await getMatterTasks(token, matter.id);
      setTasks(result);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to load tasks.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, matter.id]);

  async function onCreateTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !title.trim()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await createMatterTask(token, matter.id, {
        title: title.trim(),
        description: description.trim() || undefined,
        dueAtUtc: fromLocalInput(dueAtLocal),
        assigneeDisplayName: assigneeDisplayName.trim() || undefined,
        priority,
        isDeadlineVerified,
      });
      setTitle("");
      setDescription("");
      setDueAtLocal("");
      setAssigneeDisplayName("");
      setPriority("Normal");
      setIsDeadlineVerified(false);
      await loadTasks();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to create task.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onQuickStatusChange(task: MatterTask, status: string) {
    if (!token) {
      return;
    }

    setError(null);
    try {
      await updateMatterTask(token, matter.id, task.id, {
        title: task.title,
        description: task.description ?? undefined,
        priority: task.priority,
        status,
        assigneeActorId: task.assigneeActorId ?? undefined,
        assigneeDisplayName: task.assigneeDisplayName ?? undefined,
        dueAtUtc: task.dueAtUtc ?? undefined,
        isDeadlineVerified: task.isDeadlineVerified,
      });
      await loadTasks();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to update task status.");
    }
  }

  async function onToggleVerified(task: MatterTask, verified: boolean) {
    if (!token) {
      return;
    }

    setError(null);
    try {
      await updateMatterTask(token, matter.id, task.id, {
        title: task.title,
        description: task.description ?? undefined,
        priority: task.priority,
        status: task.status,
        assigneeActorId: task.assigneeActorId ?? undefined,
        assigneeDisplayName: task.assigneeDisplayName ?? undefined,
        dueAtUtc: task.dueAtUtc ?? undefined,
        isDeadlineVerified: verified,
      });
      await loadTasks();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to verify deadline.");
    }
  }

  async function onDeleteTask(taskId: string) {
    if (!token) {
      return;
    }

    setError(null);
    try {
      await deleteMatterTask(token, matter.id, taskId);
      await loadTasks();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to delete task.");
    }
  }

  return (
    <div className="matter-tab-panel">
      <h2>Tasks & Deadlines</h2>
      {error && <p className="error-text">{error}</p>}
      <form className="workspace-form" onSubmit={onCreateTask}>
        <label htmlFor="taskTitle">Task title</label>
        <input id="taskTitle" value={title} onChange={(event) => setTitle(event.target.value)} required />
        <label htmlFor="taskDescription">Description</label>
        <textarea id="taskDescription" rows={3} value={description} onChange={(event) => setDescription(event.target.value)} />
        <label htmlFor="taskDueAt">Due at</label>
        <input id="taskDueAt" type="datetime-local" value={dueAtLocal} onChange={(event) => setDueAtLocal(event.target.value)} />
        <label htmlFor="taskAssignee">Assignee display name</label>
        <input id="taskAssignee" value={assigneeDisplayName} onChange={(event) => setAssigneeDisplayName(event.target.value)} />
        <label htmlFor="taskPriority">Priority</label>
        <select id="taskPriority" value={priority} onChange={(event) => setPriority(event.target.value)}>
          <option value="Low">Low</option>
          <option value="Normal">Normal</option>
          <option value="High">High</option>
          <option value="Urgent">Urgent</option>
        </select>
        <label className="checkbox-field">
          <input
            type="checkbox"
            checked={isDeadlineVerified}
            onChange={(event) => setIsDeadlineVerified(event.target.checked)}
          />
          Deadline verified
        </label>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Adding..." : "Add task"}
        </button>
      </form>

      {isLoading && <p>Loading tasks...</p>}
      {!isLoading && (
        <ul className="list-reset">
          {tasks.map((task) => (
            <li key={task.id} className="party-row">
              <p>
                <strong>{task.title}</strong> • {task.status} • {task.priority}
              </p>
              <p className="muted">
                Assignee: {task.assigneeDisplayName ?? "Unassigned"} • Due:{" "}
                {task.dueAtUtc ? new Date(task.dueAtUtc).toLocaleString() : "No due date"}
              </p>
              {task.description && <p className="muted">{task.description}</p>}
              <p className="muted">
                Verified: {task.isDeadlineVerified ? "Yes" : "No"}
                {task.isDeadlineVerified && task.deadlineVerifiedByDisplayName
                  ? ` (${task.deadlineVerifiedByDisplayName})`
                  : ""}
              </p>
              <button type="button" className="mini-action" onClick={() => onQuickStatusChange(task, "InProgress")}>
                Mark In Progress
              </button>
              <button type="button" className="mini-action" onClick={() => onQuickStatusChange(task, "Completed")}>
                Mark Completed
              </button>
              <button
                type="button"
                className="mini-action"
                onClick={() => onToggleVerified(task, !task.isDeadlineVerified)}
              >
                {task.isDeadlineVerified ? "Unverify Deadline" : "Verify Deadline"}
              </button>
              <button type="button" className="danger-button" onClick={() => onDeleteTask(task.id)}>
                Delete
              </button>
            </li>
          ))}
          {tasks.length === 0 && <li className="party-row muted">No tasks yet for this matter.</li>}
        </ul>
      )}
    </div>
  );
}

export function MatterDocumentsTab() {
  const { matter } = useMatterContext();
  const { token } = useAuth();
  const [documents, setDocuments] = useState<MatterDocument[]>([]);
  const [fileName, setFileName] = useState("");
  const [documentType, setDocumentType] = useState("General");
  const [storageKey, setStorageKey] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadDocuments() {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const result = await getMatterDocuments(token, matter.id);
      setDocuments(result);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to load documents.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadDocuments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, matter.id]);

  async function onCreateDocument(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !fileName.trim()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await createMatterDocument(token, matter.id, {
        fileName: fileName.trim(),
        documentType,
        storageKey: storageKey.trim() || undefined,
      });
      setFileName("");
      setStorageKey("");
      setDocumentType("General");
      await loadDocuments();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to add document metadata.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onReviewDocument(documentId: string, reviewStatus: string) {
    if (!token) {
      return;
    }

    setError(null);
    try {
      await reviewMatterDocument(token, matter.id, documentId, {
        reviewStatus,
        reviewNotes: reviewStatus === "Approved" ? "Reviewed and approved." : "Needs follow-up.",
      });
      await loadDocuments();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to review document.");
    }
  }

  async function onDeleteDocument(documentId: string) {
    if (!token) {
      return;
    }

    setError(null);
    try {
      await deleteMatterDocument(token, matter.id, documentId);
      await loadDocuments();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to delete document metadata.");
    }
  }

  return (
    <div className="matter-tab-panel">
      <h2>Documents</h2>
      {error && <p className="error-text">{error}</p>}
      <form className="workspace-form" onSubmit={onCreateDocument}>
        <label htmlFor="documentFileName">File name</label>
        <input id="documentFileName" value={fileName} onChange={(event) => setFileName(event.target.value)} required />
        <label htmlFor="documentType">Document type</label>
        <select id="documentType" value={documentType} onChange={(event) => setDocumentType(event.target.value)}>
          <option value="General">General</option>
          <option value="Retainer">Retainer</option>
          <option value="Pleading">Pleading</option>
          <option value="Evidence">Evidence</option>
          <option value="Correspondence">Correspondence</option>
        </select>
        <label htmlFor="documentStorageKey">Storage key / URI</label>
        <input id="documentStorageKey" value={storageKey} onChange={(event) => setStorageKey(event.target.value)} />
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Adding..." : "Add document entry"}
        </button>
      </form>

      {isLoading && <p>Loading documents...</p>}
      {!isLoading && (
        <ul className="list-reset">
          {documents.map((document) => (
            <li key={document.id} className="party-row">
              <p>
                <strong>{document.fileName}</strong> • {document.documentType} • {document.reviewStatus}
              </p>
              <p className="muted">
                Uploaded by {document.uploadedByDisplayName} on {new Date(document.uploadedAtUtc).toLocaleString()}
              </p>
              {document.storageKey && <p className="muted">{document.storageKey}</p>}
              {document.reviewNotes && <p className="muted">Review: {document.reviewNotes}</p>}
              <button type="button" className="mini-action" onClick={() => onReviewDocument(document.id, "Approved")}>
                Approve
              </button>
              <button type="button" className="mini-action" onClick={() => onReviewDocument(document.id, "NeedsRevision")}>
                Needs Revision
              </button>
              <button type="button" className="danger-button" onClick={() => onDeleteDocument(document.id)}>
                Delete
              </button>
            </li>
          ))}
          {documents.length === 0 && <li className="party-row muted">No document entries yet for this matter.</li>}
        </ul>
      )}
    </div>
  );
}

export function MatterTimelineTab() {
  const { matter } = useMatterContext();
  const { token } = useAuth();
  const [timelineActions, setTimelineActions] = useState<MatterTimelineAction[]>([]);
  const [actionType, setActionType] = useState("Note");
  const [summary, setSummary] = useState("");
  const [details, setDetails] = useState("");
  const [occurredAtLocal, setOccurredAtLocal] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadTimelineActions() {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const result = await getMatterTimelineActions(token, matter.id);
      setTimelineActions(result);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to load timeline actions.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadTimelineActions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, matter.id]);

  async function onCreateAction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !summary.trim()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await createMatterTimelineAction(token, matter.id, {
        actionType,
        summary: summary.trim(),
        details: details.trim() || undefined,
        occurredAtUtc: fromLocalInput(occurredAtLocal),
      });
      setActionType("Note");
      setSummary("");
      setDetails("");
      setOccurredAtLocal("");
      await loadTimelineActions();
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "Failed to add timeline action.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="matter-tab-panel">
      <h2>Timeline</h2>
      {error && <p className="error-text">{error}</p>}

      <form className="workspace-form" onSubmit={onCreateAction}>
        <label htmlFor="timelineActionType">Action type</label>
        <select id="timelineActionType" value={actionType} onChange={(event) => setActionType(event.target.value)}>
          <option value="Note">Note</option>
          <option value="ClientCall">Client Call</option>
          <option value="Meeting">Meeting</option>
          <option value="Filing">Filing</option>
          <option value="Settlement">Settlement</option>
        </select>
        <label htmlFor="timelineSummary">Summary</label>
        <input id="timelineSummary" value={summary} onChange={(event) => setSummary(event.target.value)} required />
        <label htmlFor="timelineDetails">Details</label>
        <textarea id="timelineDetails" rows={3} value={details} onChange={(event) => setDetails(event.target.value)} />
        <label htmlFor="timelineOccurredAt">Occurred at</label>
        <input
          id="timelineOccurredAt"
          type="datetime-local"
          value={occurredAtLocal}
          onChange={(event) => setOccurredAtLocal(event.target.value)}
        />
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Adding..." : "Add timeline action"}
        </button>
      </form>

      {isLoading && <p>Loading timeline...</p>}
      {!isLoading && (
        <ul className="list-reset">
          {timelineActions.map((action) => (
            <li key={action.id} className="party-row">
              <p>
                <strong>{action.actionType}</strong>: {action.summary}
              </p>
              <p className="muted">
                {action.createdByDisplayName} • {new Date(action.occurredAtUtc).toLocaleString()}
              </p>
              {action.details && <p className="muted">{action.details}</p>}
            </li>
          ))}

          {matter.stageHistory.map((history) => (
            <li key={history.id} className="party-row">
              <p>
                <strong>Stage Change</strong>: {history.fromStage} → {history.toStage}
              </p>
              <p className="muted">
                {history.changedByDisplayName} • {new Date(history.changedAtUtc).toLocaleString()}
              </p>
              {history.changeReason && <p className="muted">{history.changeReason}</p>}
            </li>
          ))}

          {timelineActions.length === 0 && matter.stageHistory.length === 0 && (
            <li className="party-row muted">No timeline entries yet.</li>
          )}
        </ul>
      )}
    </div>
  );
}

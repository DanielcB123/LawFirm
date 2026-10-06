import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import {
  createCalendarEntry,
  deleteCalendarEntry,
  getCalendarEntries,
  updateCalendarEntry,
} from "../services/calendarApi";
import type { CalendarEntry } from "../types/calendar";

type CalendarView = "month" | "week" | "day";
type ModalMode = "create" | "edit";

const entryTypeOptions: Array<CalendarEntry["entryType"]> = ["Task", "Event", "Deadline"];
const deadlineTypeOptions: Array<CalendarEntry["deadlineType"]> = [
  "OrdinaryTask",
  "InternalTarget",
  "CourtOrdered",
  "LegallySignificant",
];
const quarterSlots = Array.from({ length: 96 }, (_, index) => index);

function startOfDay(value: Date): Date {
  const date = new Date(value);
  date.setHours(0, 0, 0, 0);
  return date;
}

function startOfWeek(value: Date): Date {
  const date = startOfDay(value);
  const day = date.getDay();
  date.setDate(date.getDate() - day);
  return date;
}

function toLocalInputValue(value: Date): string {
  const copy = new Date(value);
  const year = copy.getFullYear();
  const month = String(copy.getMonth() + 1).padStart(2, "0");
  const day = String(copy.getDate()).padStart(2, "0");
  const hours = String(copy.getHours()).padStart(2, "0");
  const minutes = String(copy.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function fromQuarterSlot(baseDate: Date, slot: number): Date {
  const date = startOfDay(baseDate);
  const hours = Math.floor(slot / 4);
  const minutes = (slot % 4) * 15;
  date.setHours(hours, minutes, 0, 0);
  return date;
}

function formatQuarterLabel(slot: number): string {
  const hours = Math.floor(slot / 4);
  const minutes = (slot % 4) * 15;
  return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}`;
}

export function TasksCalendarPage() {
  const { token, actor } = useAuth();
  const [entries, setEntries] = useState<CalendarEntry[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [view, setView] = useState<CalendarView>("week");
  const [cursorDate, setCursorDate] = useState(new Date());

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<ModalMode>("create");
  const [selectedEntryId, setSelectedEntryId] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [entryType, setEntryType] = useState<CalendarEntry["entryType"]>("Task");
  const [deadlineType, setDeadlineType] = useState<CalendarEntry["deadlineType"]>("InternalTarget");
  const [scheduledAtLocal, setScheduledAtLocal] = useState("");
  const [backupActorId, setBackupActorId] = useState("");
  const [backupDisplayName, setBackupDisplayName] = useState("");
  const [matterReference, setMatterReference] = useState("");
  const [sourceReference, setSourceReference] = useState("");
  const [reminderOffsetsMinutes, setReminderOffsetsMinutes] = useState("1440, 60");
  const canReadCalendar = (actor?.permissions ?? []).includes("calendar.read");

  async function loadEntries() {
    if (!token || !canReadCalendar) {
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const result = await getCalendarEntries(token);
      setEntries(result);
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to load calendar entries.";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadEntries();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, canReadCalendar]);

  const sortedEntries = useMemo(
    () =>
      [...entries].sort(
        (left, right) => new Date(left.scheduledAtUtc).getTime() - new Date(right.scheduledAtUtc).getTime(),
      ),
    [entries],
  );

  const monthStart = useMemo(() => {
    const value = new Date(cursorDate.getFullYear(), cursorDate.getMonth(), 1);
    return startOfWeek(value);
  }, [cursorDate]);
  const monthDays = useMemo(
    () => Array.from({ length: 42 }, (_, index) => new Date(monthStart.getFullYear(), monthStart.getMonth(), monthStart.getDate() + index)),
    [monthStart],
  );
  const weekStart = useMemo(() => startOfWeek(cursorDate), [cursorDate]);
  const weekDays = useMemo(
    () => Array.from({ length: 7 }, (_, index) => new Date(weekStart.getFullYear(), weekStart.getMonth(), weekStart.getDate() + index)),
    [weekStart],
  );

  function previousRange() {
    const date = new Date(cursorDate);
    if (view === "month") {
      date.setMonth(date.getMonth() - 1);
    } else if (view === "week") {
      date.setDate(date.getDate() - 7);
    } else {
      date.setDate(date.getDate() - 1);
    }
    setCursorDate(date);
  }

  function nextRange() {
    const date = new Date(cursorDate);
    if (view === "month") {
      date.setMonth(date.getMonth() + 1);
    } else if (view === "week") {
      date.setDate(date.getDate() + 7);
    } else {
      date.setDate(date.getDate() + 1);
    }
    setCursorDate(date);
  }

  function openCreateModal(date: Date) {
    setModalMode("create");
    setSelectedEntryId(null);
    setTitle("");
    setEntryType("Task");
    setDeadlineType("InternalTarget");
    setScheduledAtLocal(toLocalInputValue(date));
    setBackupActorId("");
    setBackupDisplayName("");
    setMatterReference("");
    setSourceReference("");
    setReminderOffsetsMinutes("1440, 60");
    setIsModalOpen(true);
  }

  function openEditModal(entry: CalendarEntry) {
    setModalMode("edit");
    setSelectedEntryId(entry.id);
    setTitle(entry.title);
    setEntryType(entry.entryType);
    setDeadlineType(entry.deadlineType);
    setScheduledAtLocal(toLocalInputValue(new Date(entry.scheduledAtUtc)));
    setBackupActorId(entry.backupActorId ?? "");
    setBackupDisplayName(entry.backupDisplayName ?? "");
    setMatterReference(entry.matterReference ?? "");
    setSourceReference(entry.sourceReference ?? "");
    setReminderOffsetsMinutes((entry.reminderOffsetsMinutes ?? []).join(", "));
    setIsModalOpen(true);
  }

  function closeModal() {
    setIsModalOpen(false);
    setSelectedEntryId(null);
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !actor) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    const payload = {
      title,
      entryType,
      deadlineType,
      ownerActorId: actor.id,
      ownerDisplayName: actor.displayName,
      backupActorId: backupActorId || undefined,
      backupDisplayName: backupDisplayName || undefined,
      scheduledAtUtc: new Date(scheduledAtLocal).toISOString(),
      isAllDay: false,
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC",
      matterReference: matterReference || undefined,
      sourceReference: sourceReference || undefined,
      reminderOffsetsMinutes: reminderOffsetsMinutes
        .split(",")
        .map((value) => Number(value.trim()))
        .filter((value) => Number.isFinite(value) && value >= 0),
    };

    try {
      if (modalMode === "create") {
        await createCalendarEntry(token, payload);
      } else if (selectedEntryId) {
        await updateCalendarEntry(token, selectedEntryId, payload);
      }

      closeModal();
      await loadEntries();
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to save entry.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onDeleteEntry() {
    if (!token || !selectedEntryId) {
      return;
    }

    setIsSubmitting(true);
    setError(null);
    try {
      await deleteCalendarEntry(token, selectedEntryId);
      closeModal();
      await loadEntries();
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to delete entry.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  function entriesForDate(date: Date): CalendarEntry[] {
    const dayStart = startOfDay(date);
    const nextDay = new Date(dayStart);
    nextDay.setDate(nextDay.getDate() + 1);

    return sortedEntries.filter((entry) => {
      const scheduled = new Date(entry.scheduledAtUtc);
      return scheduled >= dayStart && scheduled < nextDay;
    });
  }

  function entryForQuarter(date: Date, slot: number): CalendarEntry[] {
    const slotStart = fromQuarterSlot(date, slot);
    const slotEnd = new Date(slotStart);
    slotEnd.setMinutes(slotEnd.getMinutes() + 15);
    return sortedEntries.filter((entry) => {
      const scheduled = new Date(entry.scheduledAtUtc);
      return scheduled >= slotStart && scheduled < slotEnd;
    });
  }

  const periodLabel =
    view === "month"
      ? cursorDate.toLocaleDateString(undefined, { month: "long", year: "numeric" })
      : view === "week"
        ? `${weekDays[0].toLocaleDateString()} - ${weekDays[6].toLocaleDateString()}`
        : cursorDate.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric", year: "numeric" });

  if (!canReadCalendar) {
    return (
      <section className="page">
        <h1>Tasks & Calendar</h1>
        <article className="card">
          <h2>Access required</h2>
          <p className="lead">
            Your current role does not include calendar access. Sign in with an authorized staff account
            (paralegal, attorney, or admin) to view and manage tasks and deadlines.
          </p>
        </article>
      </section>
    );
  }

  return (
    <section className="page">
      <h1>Tasks & Calendar</h1>
      <p className="lead">
        Month, week, and day calendar views with quarter-hour scheduling. Click any date/time slot to create a task,
        or click an existing item to edit or delete it.
      </p>

      <article className="card">
        <div className="calendar-toolbar">
          <div className="calendar-tablist">
            {(["month", "week", "day"] as CalendarView[]).map((option) => (
              <button
                key={option}
                type="button"
                className={`calendar-tab ${view === option ? "calendar-tab--active" : ""}`}
                onClick={() => setView(option)}
              >
                {option[0].toUpperCase() + option.slice(1)}
              </button>
            ))}
          </div>

          <div className="calendar-nav">
            <button type="button" className="mini-action" onClick={previousRange}>
              Prev
            </button>
            <button type="button" className="mini-action" onClick={() => setCursorDate(new Date())}>
              Today
            </button>
            <button type="button" className="mini-action" onClick={nextRange}>
              Next
            </button>
          </div>
        </div>

        <p className="calendar-period">{periodLabel}</p>

        {error && <p className="error-text">{error}</p>}
        {isLoading && <p>Loading calendar...</p>}

        {!isLoading && view === "month" && (
          <div className="month-grid">
            {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((dayName) => (
              <div key={dayName} className="month-grid__head">
                {dayName}
              </div>
            ))}
            {monthDays.map((day) => {
              const dayEntries = entriesForDate(day);
              const isCurrentMonth = day.getMonth() === cursorDate.getMonth();
              return (
                <button
                  key={day.toISOString()}
                  type="button"
                  className={`month-cell ${isCurrentMonth ? "" : "month-cell--faded"}`}
                  onClick={() => openCreateModal(new Date(day.getFullYear(), day.getMonth(), day.getDate(), 9, 0))}
                >
                  <span className="month-cell__date">{day.getDate()}</span>
                  <span className="month-cell__events">
                    {dayEntries.slice(0, 3).map((entry) => (
                      <span
                        key={entry.id}
                        className="calendar-chip"
                        onClick={(event) => {
                          event.stopPropagation();
                          openEditModal(entry);
                        }}
                      >
                        {new Date(entry.scheduledAtUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}{" "}
                        {entry.title}
                      </span>
                    ))}
                  </span>
                </button>
              );
            })}
          </div>
        )}

        {!isLoading && view === "week" && (
          <div className="calendar-scroll">
            <table className="calendar-table">
              <thead>
                <tr>
                  <th>Time</th>
                  {weekDays.map((day) => (
                    <th key={day.toISOString()}>{day.toLocaleDateString(undefined, { weekday: "short", month: "numeric", day: "numeric" })}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {quarterSlots.map((slot) => (
                  <tr key={slot}>
                    <td className={slot % 4 === 0 ? "time-cell time-cell--hour" : "time-cell"}>
                      {formatQuarterLabel(slot)}
                    </td>
                    {weekDays.map((day) => {
                      const slotEntries = entryForQuarter(day, slot);
                      return (
                        <td
                          key={`${day.toISOString()}-${slot}`}
                          className={slot % 4 === 0 ? "slot-cell slot-cell--hour" : "slot-cell"}
                          onClick={() => openCreateModal(fromQuarterSlot(day, slot))}
                        >
                          {slotEntries.map((entry) => (
                            <button
                              key={entry.id}
                              type="button"
                              className="calendar-chip"
                              onClick={(event) => {
                                event.stopPropagation();
                                openEditModal(entry);
                              }}
                            >
                              {entry.title}
                            </button>
                          ))}
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {!isLoading && view === "day" && (
          <div className="calendar-scroll">
            <table className="calendar-table calendar-table--day">
              <thead>
                <tr>
                  <th>Time</th>
                  <th>{cursorDate.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" })}</th>
                </tr>
              </thead>
              <tbody>
                {quarterSlots.map((slot) => {
                  const slotEntries = entryForQuarter(cursorDate, slot);
                  return (
                    <tr key={slot}>
                      <td className={slot % 4 === 0 ? "time-cell time-cell--hour" : "time-cell"}>
                        {formatQuarterLabel(slot)}
                      </td>
                      <td
                        className={slot % 4 === 0 ? "slot-cell slot-cell--hour" : "slot-cell"}
                        onClick={() => openCreateModal(fromQuarterSlot(cursorDate, slot))}
                      >
                        {slotEntries.map((entry) => (
                          <button
                            key={entry.id}
                            type="button"
                            className="calendar-chip"
                            onClick={(event) => {
                              event.stopPropagation();
                              openEditModal(entry);
                            }}
                          >
                            {entry.title}
                          </button>
                        ))}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </article>

      {isModalOpen && (
        <div className="modal-backdrop" role="presentation" onClick={closeModal}>
          <div className="modal-card" role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
            <h2>{modalMode === "create" ? "Create calendar task" : "Edit calendar task"}</h2>
            <form className="workspace-form" onSubmit={onSubmit}>
              <label htmlFor="modal-title">Title</label>
              <input id="modal-title" value={title} onChange={(event) => setTitle(event.target.value)} required />

              <label htmlFor="modal-entry-type">Entry type</label>
              <select
                id="modal-entry-type"
                value={entryType}
                onChange={(event) => setEntryType(event.target.value as CalendarEntry["entryType"])}
              >
                {entryTypeOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>

              <label htmlFor="modal-deadline-type">Deadline classification</label>
              <select
                id="modal-deadline-type"
                value={deadlineType}
                onChange={(event) => setDeadlineType(event.target.value as CalendarEntry["deadlineType"])}
              >
                {deadlineTypeOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>

              <label htmlFor="modal-scheduled-at">Scheduled at</label>
              <input
                id="modal-scheduled-at"
                type="datetime-local"
                value={scheduledAtLocal}
                onChange={(event) => setScheduledAtLocal(event.target.value)}
                required
              />

              <label htmlFor="modal-backup-id">Backup actor ID (optional)</label>
              <input
                id="modal-backup-id"
                value={backupActorId}
                onChange={(event) => setBackupActorId(event.target.value)}
              />

              <label htmlFor="modal-backup-name">Backup display name (optional)</label>
              <input
                id="modal-backup-name"
                value={backupDisplayName}
                onChange={(event) => setBackupDisplayName(event.target.value)}
              />

              <label htmlFor="modal-matter-reference">Matter reference (optional)</label>
              <input
                id="modal-matter-reference"
                value={matterReference}
                onChange={(event) => setMatterReference(event.target.value)}
              />

              <label htmlFor="modal-source-reference">Source reference (optional)</label>
              <input
                id="modal-source-reference"
                value={sourceReference}
                onChange={(event) => setSourceReference(event.target.value)}
              />

              <label htmlFor="modal-reminders">Reminder offsets minutes</label>
              <input
                id="modal-reminders"
                value={reminderOffsetsMinutes}
                onChange={(event) => setReminderOffsetsMinutes(event.target.value)}
              />

              <div className="modal-actions">
                {modalMode === "edit" && (
                  <button type="button" className="danger-button" disabled={isSubmitting} onClick={onDeleteEntry}>
                    Delete
                  </button>
                )}
                <button type="button" className="mini-action" onClick={closeModal}>
                  Cancel
                </button>
                <button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Saving..." : modalMode === "create" ? "Create" : "Save changes"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}

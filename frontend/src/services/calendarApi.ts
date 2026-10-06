import { deleteRequest, getJson, postJson, putJson } from "./apiClient";
import type { CalendarEntry, CreateCalendarEntryPayload } from "../types/calendar";

export async function getCalendarEntries(token: string): Promise<CalendarEntry[]> {
  return getJson<CalendarEntry[]>("/api/calendar/entries", undefined, token);
}

export async function createCalendarEntry(
  token: string,
  payload: CreateCalendarEntryPayload,
): Promise<CalendarEntry> {
  return postJson<CalendarEntry, CreateCalendarEntryPayload>("/api/calendar/entries", payload, token);
}

export async function updateCalendarEntry(
  token: string,
  entryId: string,
  payload: CreateCalendarEntryPayload,
): Promise<CalendarEntry> {
  return putJson<CalendarEntry, CreateCalendarEntryPayload>(`/api/calendar/entries/${entryId}`, payload, token);
}

export async function deleteCalendarEntry(token: string, entryId: string): Promise<void> {
  await deleteRequest(`/api/calendar/entries/${entryId}`, token);
}

export async function acknowledgeCalendarEntry(token: string, entryId: string): Promise<void> {
  await postJson<Record<string, never>, Record<string, never>>(`/api/calendar/entries/${entryId}/acknowledge`, {}, token);
}

export async function completeCalendarEntry(token: string, entryId: string): Promise<void> {
  await postJson<Record<string, never>, Record<string, never>>(`/api/calendar/entries/${entryId}/complete`, {}, token);
}

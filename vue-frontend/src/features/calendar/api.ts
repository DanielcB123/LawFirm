import { getJson, postJson } from '../../lib/apiClient'
import type { CalendarEntry, CreateCalendarEntryPayload } from './types'

type CalendarListFilters = {
  fromUtc?: string
  toUtc?: string
  entryType?: 'Task' | 'Event' | 'Deadline'
  includeCompleted?: boolean
}

export async function getCalendarEntries(token: string, filters: CalendarListFilters = {}): Promise<CalendarEntry[]> {
  const params = new URLSearchParams()
  if (filters.fromUtc) {
    params.set('fromUtc', filters.fromUtc)
  }
  if (filters.toUtc) {
    params.set('toUtc', filters.toUtc)
  }
  if (filters.entryType) {
    params.set('entryType', filters.entryType)
  }
  if (filters.includeCompleted) {
    params.set('includeCompleted', '1')
  }

  const query = params.size > 0 ? `?${params.toString()}` : ''
  return getJson<CalendarEntry[]>(`/tenant/calendar/entries${query}`, token)
}

export async function createCalendarEntry(
  token: string,
  payload: CreateCalendarEntryPayload,
): Promise<CalendarEntry> {
  return postJson<CalendarEntry, CreateCalendarEntryPayload>('/tenant/calendar/entries', payload, token)
}

export async function completeCalendarEntry(token: string, entryId: string, isCompleted = true): Promise<CalendarEntry> {
  return postJson<CalendarEntry, { isCompleted: boolean }>(`/tenant/calendar/entries/${entryId}/complete`, { isCompleted }, token)
}

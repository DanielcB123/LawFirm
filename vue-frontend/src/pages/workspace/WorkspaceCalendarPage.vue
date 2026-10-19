<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { completeCalendarEntry, createCalendarEntry, getCalendarEntries } from '../../features/calendar/api'
import type { CalendarEntry } from '../../features/calendar/types'

type CalendarView = 'week' | 'day' | 'month'

const WEEKDAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const TIME_SLOT_MINUTES = 30
const TIME_SLOT_COUNT = (24 * 60) / TIME_SLOT_MINUTES

const auth = useAuthSession()
const entries = ref<CalendarEntry[]>([])
const error = ref<string | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const activeView = ref<CalendarView>('week')
const cursorDate = ref(new Date())
const selectedDate = ref(startOfDay(new Date()))
const showCompleted = ref(false)
const showCreateModal = ref(false)
const createDraft = ref(defaultCreateDraft())

const canManage = computed(() => auth.hasPermission('calendar.manage'))

function defaultCreateDraft() {
  const start = new Date()
  start.setMinutes(start.getMinutes() - (start.getMinutes() % TIME_SLOT_MINUTES), 0, 0)
  const end = new Date(start)
  end.setMinutes(end.getMinutes() + 30)

  return {
    title: '',
    entryType: 'Event' as CalendarEntry['entryType'],
    deadlineType: 'InternalTarget' as CalendarEntry['deadlineType'],
    startLocal: toLocalInputValue(start),
    endLocal: toLocalInputValue(end),
    location: '',
    matterReference: '',
    description: '',
    allDay: false,
  }
}

function startOfDay(value: Date): Date {
  const normalized = new Date(value)
  normalized.setHours(0, 0, 0, 0)
  return normalized
}

function startOfWeek(value: Date): Date {
  const normalized = startOfDay(value)
  normalized.setDate(normalized.getDate() - normalized.getDay())
  return normalized
}

function toDateKey(value: Date): string {
  const year = value.getFullYear()
  const month = String(value.getMonth() + 1).padStart(2, '0')
  const day = String(value.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function toLocalInputValue(value: Date): string {
  const year = value.getFullYear()
  const month = String(value.getMonth() + 1).padStart(2, '0')
  const day = String(value.getDate()).padStart(2, '0')
  const hours = String(value.getHours()).padStart(2, '0')
  const minutes = String(value.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

function monthGridDays(anchor: Date): Date[] {
  const monthStart = startOfWeek(new Date(anchor.getFullYear(), anchor.getMonth(), 1))
  return Array.from({ length: 42 }, (_, index) => {
    return new Date(monthStart.getFullYear(), monthStart.getMonth(), monthStart.getDate() + index)
  })
}

const weekDays = computed(() =>
  Array.from({ length: 7 }, (_, index) => {
    const base = startOfWeek(cursorDate.value)
    return new Date(base.getFullYear(), base.getMonth(), base.getDate() + index)
  }),
)

const navigatorDays = computed(() => monthGridDays(cursorDate.value))
const navigatorLabel = computed(() =>
  cursorDate.value.toLocaleDateString(undefined, { month: 'long', year: 'numeric' }),
)

const timeSlots = computed(() =>
  Array.from({ length: TIME_SLOT_COUNT }, (_, index) => {
    const minuteOfDay = index * TIME_SLOT_MINUTES
    const hours = Math.floor(minuteOfDay / 60)
    const minutes = minuteOfDay % 60
    const marker = new Date()
    marker.setHours(hours, minutes, 0, 0)
    return {
      minuteOfDay,
      label: marker.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }),
    }
  }),
)

const entriesByDate = computed(() => {
  const grouped = new Map<string, CalendarEntry[]>()
  for (const entry of entries.value) {
    const scheduled = new Date(entry.scheduledAtUtc)
    const key = toDateKey(scheduled)
    const existing = grouped.get(key)
    if (existing) {
      existing.push(entry)
    } else {
      grouped.set(key, [entry])
    }
  }

  for (const [key, group] of grouped.entries()) {
    grouped.set(
      key,
      group.sort((left, right) => new Date(left.scheduledAtUtc).getTime() - new Date(right.scheduledAtUtc).getTime()),
    )
  }

  return grouped
})

const entriesBySlot = computed(() => {
  const grouped = new Map<string, CalendarEntry[]>()
  for (const entry of entries.value) {
    const scheduled = new Date(entry.scheduledAtUtc)
    const minuteOfDay = scheduled.getHours() * 60 + scheduled.getMinutes()
    const slotMinute = minuteOfDay - (minuteOfDay % TIME_SLOT_MINUTES)
    const slotKey = `${toDateKey(scheduled)}|${slotMinute}`
    const existing = grouped.get(slotKey)
    if (existing) {
      existing.push(entry)
    } else {
      grouped.set(slotKey, [entry])
    }
  }
  return grouped
})

const selectedDateEntries = computed(() => {
  return entriesForDate(selectedDate.value)
})

const periodLabel = computed(() => {
  if (activeView.value === 'month') {
    return cursorDate.value.toLocaleDateString(undefined, { month: 'long', year: 'numeric' })
  }

  if (activeView.value === 'week') {
    const [first, last] = [weekDays.value[0], weekDays.value[6]]
    return `${first.toLocaleDateString()} - ${last.toLocaleDateString()}`
  }

  return cursorDate.value.toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' })
})

function isToday(value: Date): boolean {
  return toDateKey(value) === toDateKey(new Date())
}

function isSelectedDate(value: Date): boolean {
  return toDateKey(value) === toDateKey(selectedDate.value)
}

function formatDayHeading(value: Date): string {
  return value.toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' })
}

function entriesForDate(date: Date): CalendarEntry[] {
  return entriesByDate.value.get(toDateKey(date)) ?? []
}

function entriesForSlot(date: Date, minuteOfDay: number): CalendarEntry[] {
  return entriesBySlot.value.get(`${toDateKey(date)}|${minuteOfDay}`) ?? []
}

function moveRange(direction: -1 | 1) {
  const updated = new Date(cursorDate.value)
  if (activeView.value === 'month') {
    updated.setMonth(updated.getMonth() + direction)
  } else if (activeView.value === 'week') {
    updated.setDate(updated.getDate() + direction * 7)
  } else {
    updated.setDate(updated.getDate() + direction)
  }
  cursorDate.value = updated
  selectedDate.value = startOfDay(updated)
}

function jumpToToday() {
  const today = new Date()
  cursorDate.value = today
  selectedDate.value = startOfDay(today)
}

function moveNavigatorMonth(direction: -1 | 1) {
  const updated = new Date(cursorDate.value)
  updated.setMonth(updated.getMonth() + direction)
  cursorDate.value = updated
}

function pickDate(date: Date) {
  const next = startOfDay(date)
  cursorDate.value = next
  selectedDate.value = next
}

function openCreateModal(start: Date) {
  const roundedStart = new Date(start)
  roundedStart.setMinutes(roundedStart.getMinutes() - (roundedStart.getMinutes() % TIME_SLOT_MINUTES), 0, 0)
  const roundedEnd = new Date(roundedStart)
  roundedEnd.setMinutes(roundedEnd.getMinutes() + 30)

  createDraft.value = {
    ...defaultCreateDraft(),
    startLocal: toLocalInputValue(roundedStart),
    endLocal: toLocalInputValue(roundedEnd),
  }
  showCreateModal.value = true
}

function openCreateModalForSlot(date: Date, minuteOfDay: number) {
  const slotStart = new Date(date)
  slotStart.setHours(Math.floor(minuteOfDay / 60), minuteOfDay % 60, 0, 0)
  selectedDate.value = startOfDay(slotStart)
  openCreateModal(slotStart)
}

function closeCreateModal() {
  showCreateModal.value = false
}

function encodeSourceReference(): string | undefined {
  const parts: string[] = []
  if (createDraft.value.allDay) {
    parts.push('allDay:1')
  }
  if (createDraft.value.location.trim()) {
    parts.push(`loc:${createDraft.value.location.trim()}`)
  }
  if (createDraft.value.endLocal) {
    const endIso = new Date(createDraft.value.endLocal).toISOString()
    parts.push(`end:${endIso}`)
  }
  if (createDraft.value.description.trim()) {
    const compact = createDraft.value.description.trim().replace(/\s+/g, ' ')
    parts.push(`note:${compact}`)
  }

  if (parts.length === 0) {
    return undefined
  }

  return parts.join(' | ').slice(0, 120)
}

async function loadEntries() {
  const token = auth.token.value
  if (!token || !auth.hasPermission('calendar.read')) {
    entries.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    entries.value = await getCalendarEntries(token, { includeCompleted: showCompleted.value })
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load calendar.'
  } finally {
    isLoading.value = false
  }
}

async function submitEntry() {
  const token = auth.token.value
  if (!token || !canManage.value || !createDraft.value.startLocal || !createDraft.value.title.trim()) {
    return
  }

  isSaving.value = true
  error.value = null
  try {
    await createCalendarEntry(token, {
      title: createDraft.value.title.trim(),
      entryType: createDraft.value.entryType,
      deadlineType: createDraft.value.deadlineType,
      scheduledAtUtc: new Date(createDraft.value.startLocal).toISOString(),
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
      matterReference: createDraft.value.matterReference.trim() || undefined,
      sourceReference: encodeSourceReference(),
      reminderOffsetsMinutes: [1440, 60],
    })
    showCreateModal.value = false
    await loadEntries()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create calendar entry.'
  } finally {
    isSaving.value = false
  }
}

async function markComplete(entry: CalendarEntry) {
  const token = auth.token.value
  if (!token || !canManage.value) {
    return
  }

  try {
    await completeCalendarEntry(token, entry.id, !entry.isCompleted)
    await loadEntries()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to update calendar item.'
  }
}

watch(showCreateModal, (isOpen) => {
  document.body.classList.toggle('modal-open', isOpen)
})

onMounted(() => {
  selectedDate.value = startOfDay(cursorDate.value)
  void loadEntries()
})

onBeforeUnmount(() => {
  document.body.classList.remove('modal-open')
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Calendar</p>
      <h1>Calendar and deadlines</h1>
      <p class="muted">Week view opens by default with a time grid for precise scheduling. Double-click any slot to add an event.</p>
    </header>

    <article class="card calendar-shell">
      <div class="workspace-actions">
        <div class="calendar-tabs">
          <button type="button" :class="['mini-action', { 'mini-action--active': activeView === 'week' }]" @click="activeView = 'week'">
            Week
          </button>
          <button type="button" :class="['mini-action', { 'mini-action--active': activeView === 'day' }]" @click="activeView = 'day'">
            Day
          </button>
          <button type="button" :class="['mini-action', { 'mini-action--active': activeView === 'month' }]" @click="activeView = 'month'">
            Month
          </button>
        </div>

        <button type="button" class="mini-action" @click="moveRange(-1)">Prev</button>
        <button type="button" class="mini-action" @click="jumpToToday">Today</button>
        <button type="button" class="mini-action" @click="moveRange(1)">Next</button>
        <button v-if="canManage" type="button" class="mini-action mini-action--active" @click="openCreateModal(new Date())">New event</button>

        <label class="workspace-form__inline">
          <input v-model="showCompleted" type="checkbox" @change="loadEntries" />
          Show completed
        </label>
      </div>

      <p class="calendar-period-label">{{ periodLabel }}</p>
      <p v-if="isLoading" class="muted">Loading calendar...</p>

      <div v-if="!isLoading" class="calendar-shell__content">
        <aside class="calendar-month-sidebar">
          <div class="calendar-month-sidebar__header">
            <h3>{{ navigatorLabel }}</h3>
            <div class="calendar-month-sidebar__nav">
              <button type="button" class="mini-action" @click="moveNavigatorMonth(-1)" aria-label="Previous month">&lt;</button>
              <button type="button" class="mini-action" @click="moveNavigatorMonth(1)" aria-label="Next month">&gt;</button>
            </div>
          </div>
          <div class="calendar-month-sidebar__weekdays">
            <span v-for="weekday in WEEKDAY_NAMES" :key="weekday">{{ weekday }}</span>
          </div>
          <div class="calendar-month-sidebar__days">
            <button
              v-for="day in navigatorDays"
              :key="day.toISOString()"
              type="button"
              :class="[
                'calendar-month-sidebar__day',
                { 'calendar-month-sidebar__day--outside': day.getMonth() !== cursorDate.getMonth() },
                { 'calendar-month-sidebar__day--today': isToday(day) },
                { 'calendar-month-sidebar__day--selected': isSelectedDate(day) },
              ]"
              @click="pickDate(day)"
            >
              {{ day.getDate() }}
            </button>
          </div>
        </aside>

        <div class="calendar-main">
          <div v-if="activeView === 'week'" class="calendar-time-grid calendar-time-grid--week">
            <div class="calendar-time-grid__corner"></div>
            <div v-for="day in weekDays" :key="`week-head-${day.toISOString()}`" class="calendar-time-grid__head">
              {{ formatDayHeading(day) }}
            </div>

            <template v-for="slot in timeSlots" :key="`week-slot-${slot.minuteOfDay}`">
              <div class="calendar-time-grid__time">{{ slot.label }}</div>
              <button
                v-for="day in weekDays"
                :key="`slot-${day.toISOString()}-${slot.minuteOfDay}`"
                type="button"
                class="calendar-time-grid__cell"
                @click="selectedDate = startOfDay(day)"
                @dblclick="openCreateModalForSlot(day, slot.minuteOfDay)"
              >
                <span v-for="entry in entriesForSlot(day, slot.minuteOfDay)" :key="entry.id" class="calendar-slot-entry">
                  {{ entry.title }}
                </span>
              </button>
            </template>
          </div>

          <div v-else-if="activeView === 'day'" class="calendar-time-grid calendar-time-grid--day">
            <div class="calendar-time-grid__corner"></div>
            <div class="calendar-time-grid__head">
              {{ formatDayHeading(cursorDate) }}
            </div>

            <template v-for="slot in timeSlots" :key="`day-slot-${slot.minuteOfDay}`">
              <div class="calendar-time-grid__time">{{ slot.label }}</div>
              <button
                type="button"
                class="calendar-time-grid__cell"
                @click="selectedDate = startOfDay(cursorDate)"
                @dblclick="openCreateModalForSlot(cursorDate, slot.minuteOfDay)"
              >
                <span v-for="entry in entriesForSlot(cursorDate, slot.minuteOfDay)" :key="entry.id" class="calendar-slot-entry">
                  {{ entry.title }}
                </span>
              </button>
            </template>
          </div>

          <div v-else class="calendar-month-grid">
            <div v-for="weekday in WEEKDAY_NAMES" :key="`month-head-${weekday}`" class="calendar-month-grid__head">
              {{ weekday }}
            </div>
            <button
              v-for="day in monthGridDays(cursorDate)"
              :key="`month-day-${day.toISOString()}`"
              type="button"
              :class="[
                'calendar-month-cell',
                { 'calendar-month-cell--faded': day.getMonth() !== cursorDate.getMonth() },
                { 'calendar-month-cell--selected': isSelectedDate(day) },
              ]"
              @click="pickDate(day)"
              @dblclick="openCreateModalForSlot(day, 9 * 60)"
            >
              <span class="calendar-month-cell__date">{{ day.getDate() }}</span>
              <span class="calendar-month-cell__items">
                <span v-for="entry in entriesForDate(day).slice(0, 3)" :key="entry.id" class="calendar-chip">
                  {{ new Date(entry.scheduledAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                  {{ entry.title }}
                </span>
              </span>
            </button>
          </div>

          <article class="card calendar-agenda-card">
            <h3>{{ selectedDate.toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' }) }}</h3>
            <ul class="record-list">
              <li v-for="entry in selectedDateEntries" :key="entry.id">
                <strong>{{ new Date(entry.scheduledAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} {{ entry.title }}</strong>
                <span>{{ entry.entryType }} | {{ entry.deadlineType }} | {{ entry.matterReference ?? 'General' }}</span>
                <button v-if="canManage" type="button" class="mini-action" @click="markComplete(entry)">
                  {{ entry.isCompleted ? 'Reopen' : 'Complete' }}
                </button>
              </li>
            </ul>
          </article>
        </div>
      </div>
    </article>

    <div v-if="showCreateModal" class="modal-backdrop" @click.self="closeCreateModal">
      <article class="modal-card modal-card--calendar-event">
        <header class="appointment-modal__header">
          <div>
            <p class="eyebrow">New event</p>
            <h2>Create calendar event</h2>
            <p class="muted">Capture scheduling details for consultations, hearings, deadlines, or internal tasks.</p>
          </div>
          <button type="button" class="appointment-modal__close" aria-label="Close create event modal" @click="closeCreateModal">
            x
          </button>
        </header>
        <form class="calendar-event-form" @submit.prevent="submitEntry">
          <label>
            Title *
            <input v-model="createDraft.title" type="text" required />
          </label>
          <label>
            Start time *
            <input v-model="createDraft.startLocal" type="datetime-local" required />
          </label>
          <label>
            End time
            <input v-model="createDraft.endLocal" type="datetime-local" />
          </label>
          <label>
            Entry type
            <select v-model="createDraft.entryType">
              <option value="Task">Task</option>
              <option value="Event">Event</option>
              <option value="Deadline">Deadline</option>
            </select>
          </label>
          <label>
            Deadline type
            <select v-model="createDraft.deadlineType">
              <option value="OrdinaryTask">Ordinary task</option>
              <option value="InternalTarget">Internal target</option>
              <option value="CourtOrdered">Court ordered</option>
              <option value="LegallySignificant">Legally significant</option>
            </select>
          </label>
          <label>
            Matter
            <input v-model="createDraft.matterReference" type="text" placeholder="MF-2026-1001" />
          </label>
          <label>
            Location
            <input v-model="createDraft.location" type="text" placeholder="Courtroom A or Zoom" />
          </label>
          <label class="calendar-event-form__description">
            Description
            <textarea v-model="createDraft.description" rows="4" placeholder="Case prep notes, attendees, and follow-up reminders." />
          </label>
          <label class="workspace-form__inline">
            <input v-model="createDraft.allDay" type="checkbox" />
            All day (stored as note for now)
          </label>
          <div class="modal-actions">
            <button type="button" class="mini-action" @click="closeCreateModal">Cancel</button>
            <button type="submit" class="mini-action mini-action--active" :disabled="isSaving || !canManage">
              {{ isSaving ? 'Saving...' : 'Save event' }}
            </button>
          </div>
        </form>
      </article>
    </div>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

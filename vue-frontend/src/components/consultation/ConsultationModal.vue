<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { createConsultationRequest, getAvailableConsultationSlots } from '../../features/consultations/api'
import type { ConsultationSlot } from '../../features/consultations/types'

const emit = defineEmits<{ close: [] }>()

type CalendarDay = {
  dayKey: string
  date: Date
  hasAvailability: boolean
  inCurrentMonth: boolean
  isPastDate: boolean
}

const practiceAreas = [
  { value: 'BusinessLitigation', label: 'Business litigation' },
  { value: 'CivilLitigation', label: 'Civil litigation' },
  { value: 'EstatePlanning', label: 'Estate planning' },
  { value: 'PersonalInjury', label: 'Personal injury' },
  { value: 'ComplianceAdvisory', label: 'Risk and compliance advisory' },
]
const appointmentHours = [9, 10, 11, 12, 13, 14, 15, 16]
const appointmentMinutes = [0, 30]

const modalCardRef = ref<HTMLElement | null>(null)
const firstInputRef = ref<HTMLInputElement | null>(null)
const modalScrollTopRef = ref<number | null>(null)

const visibleMonthStart = ref(startOfMonth(new Date()))
const slots = ref<ConsultationSlot[]>([])
const isLoadingSlots = ref(false)
const submitError = ref<string | null>(null)
const submitSuccess = ref<string | null>(null)
const isSubmitting = ref(false)
const selectedDayKey = ref('')
const selectedSlot = ref('')
const fullName = ref('')
const email = ref('')
const phone = ref('')
const practiceArea = ref(practiceAreas[0].value)
const message = ref('')

const calendarRange = computed(() => {
  const gridStart = new Date(visibleMonthStart.value)
  gridStart.setDate(1 - visibleMonthStart.value.getDay())
  return { gridStart, days: 42 }
})

const canGoToPreviousMonth = computed(() => {
  const currentMonth = startOfMonth(new Date())
  return visibleMonthStart.value.getTime() > currentMonth.getTime()
})

const availableSlotMapByDay = computed(() => {
  const grouped = new Map<string, Map<string, string>>()
  for (const slot of slots.value) {
    const dayKey = toLocalDayKey(slot.startsAtUtc)
    const timeKey = toLocalTimeKey(slot.startsAtUtc)
    if (!grouped.has(dayKey)) {
      grouped.set(dayKey, new Map<string, string>())
    }
    grouped.get(dayKey)?.set(timeKey, slot.startsAtUtc)
  }
  return grouped
})

const calendarDays = computed<CalendarDay[]>(() => {
  const today = new Date()
  const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime()
  const items: CalendarDay[] = []

  for (let offset = 0; offset < 42; offset += 1) {
    const date = new Date(calendarRange.value.gridStart)
    date.setDate(calendarRange.value.gridStart.getDate() + offset)
    const dayKey = toLocalDayKey(date.toISOString())
    const dateStart = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime()
    const isPastDate = dateStart < todayStart
    const hasAvailability = !isPastDate && (availableSlotMapByDay.value.get(dayKey)?.size ?? 0) > 0

    items.push({
      dayKey,
      date,
      hasAvailability,
      inCurrentMonth:
        date.getMonth() === visibleMonthStart.value.getMonth() && date.getFullYear() === visibleMonthStart.value.getFullYear(),
      isPastDate,
    })
  }

  return items
})

const activeDayKey = computed(() => {
  if (selectedDayKey.value && calendarDays.value.some((item) => item.dayKey === selectedDayKey.value)) {
    return selectedDayKey.value
  }

  return calendarDays.value.find((item) => item.hasAvailability)?.dayKey ?? calendarDays.value[0]?.dayKey ?? ''
})

const activeDayAvailableSlots = computed(() => availableSlotMapByDay.value.get(activeDayKey.value) ?? new Map<string, string>())

const selectedDaySlots = computed(() => {
  const selectedDay = calendarDays.value.find((item) => item.dayKey === activeDayKey.value)?.date
  if (!selectedDay) {
    return []
  }

  return appointmentHours.flatMap((hour) =>
    appointmentMinutes.map((minute) => {
      const slotDate = new Date(selectedDay.getFullYear(), selectedDay.getMonth(), selectedDay.getDate(), hour, minute, 0, 0)
      const timeKey = `${`${hour}`.padStart(2, '0')}:${`${minute}`.padStart(2, '0')}`
      const availableUtc = activeDayAvailableSlots.value.get(timeKey) ?? null
      return {
        timeKey,
        label: slotDate.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }),
        availableUtc,
      }
    }),
  )
})

const activeSelectedSlot = computed(() => {
  if (selectedDaySlots.value.some((slot) => slot.availableUtc === selectedSlot.value)) {
    return selectedSlot.value
  }

  return selectedDaySlots.value.find((slot) => slot.availableUtc)?.availableUtc ?? ''
})

watch([activeDayKey, selectedDaySlots], () => {
  if (!activeSelectedSlot.value) {
    selectedSlot.value = selectedDaySlots.value.find((slot) => slot.availableUtc)?.availableUtc ?? ''
  }
})

async function loadSlots() {
  isLoadingSlots.value = true
  submitError.value = null

  try {
    const available = await getAvailableConsultationSlots({
      startDate: toIsoDate(calendarRange.value.gridStart),
      days: calendarRange.value.days,
    })

    slots.value = available
    if (available.length > 0) {
      const firstSlot = available[0]
      selectedSlot.value = firstSlot.startsAtUtc
      selectedDayKey.value = toLocalDayKey(firstSlot.startsAtUtc)
    } else {
      selectedDayKey.value = toLocalDayKey(calendarRange.value.gridStart.toISOString())
      selectedSlot.value = ''
    }
  } catch (err) {
    submitError.value = err instanceof ApiClientError ? err.message : 'Unable to load appointment times.'
  } finally {
    isLoadingSlots.value = false
    await nextTick()
    if (modalScrollTopRef.value !== null && modalCardRef.value) {
      modalCardRef.value.scrollTop = modalScrollTopRef.value
      modalScrollTopRef.value = null
    }
  }
}

function closeModal() {
  emit('close')
}

function onBackdropClick() {
  closeModal()
}

function onDialogClick(event: MouseEvent) {
  event.stopPropagation()
}

function moveMonth(offset: number) {
  modalScrollTopRef.value = modalCardRef.value?.scrollTop ?? null
  visibleMonthStart.value = new Date(visibleMonthStart.value.getFullYear(), visibleMonthStart.value.getMonth() + offset, 1)
  void loadSlots()
}

function onCalendarDayKeydown(event: KeyboardEvent, index: number) {
  const keyToOffset: Record<string, number> = {
    ArrowRight: 1,
    ArrowLeft: -1,
    ArrowDown: 7,
    ArrowUp: -7,
  }

  const offset = keyToOffset[event.key]
  if (offset === undefined) {
    return
  }

  event.preventDefault()
  const buttons = Array.from(document.querySelectorAll<HTMLButtonElement>('.consultation-picker__day'))
  const next = buttons[index + offset]
  next?.focus()
}

function onTimeKeydown(event: KeyboardEvent, index: number) {
  if (!['ArrowRight', 'ArrowLeft', 'ArrowDown', 'ArrowUp'].includes(event.key)) {
    return
  }

  event.preventDefault()
  const buttons = Array.from(document.querySelectorAll<HTMLButtonElement>('.consultation-picker__time'))
  const columnCount = window.innerWidth > 760 ? 2 : 1
  const delta = event.key === 'ArrowDown' ? columnCount : event.key === 'ArrowUp' ? -columnCount : event.key === 'ArrowRight' ? 1 : -1
  buttons[index + delta]?.focus()
}

async function submitConsultation() {
  submitError.value = null
  submitSuccess.value = null

  if (!activeSelectedSlot.value) {
    submitError.value = 'Please choose an available appointment time.'
    return
  }

  isSubmitting.value = true
  try {
    await createConsultationRequest({
      fullName: fullName.value.trim(),
      email: email.value.trim(),
      phone: phone.value.trim() || undefined,
      practiceArea: practiceArea.value,
      message: message.value.trim() || undefined,
      preferredAtUtc: activeSelectedSlot.value,
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
    })
    submitSuccess.value = 'Consultation request sent. Our team will contact you shortly.'
    fullName.value = ''
    email.value = ''
    phone.value = ''
    message.value = ''
    practiceArea.value = practiceAreas[0].value
    setTimeout(() => closeModal(), 700)
  } catch (err) {
    submitError.value = err instanceof ApiClientError ? err.message : 'Unable to submit consultation request.'
  } finally {
    isSubmitting.value = false
  }
}

function onEscape(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    closeModal()
  }
}

onMounted(async () => {
  document.body.classList.add('modal-open')
  document.addEventListener('keydown', onEscape)
  await loadSlots()
  await nextTick()
  firstInputRef.value?.focus()
})

onBeforeUnmount(() => {
  document.body.classList.remove('modal-open')
  document.removeEventListener('keydown', onEscape)
})

function toLocalDayKey(dateValue: string): string {
  const date = new Date(dateValue)
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

function toLocalTimeKey(dateValue: string): string {
  const date = new Date(dateValue)
  const hour = `${date.getHours()}`.padStart(2, '0')
  const minute = `${date.getMinutes()}`.padStart(2, '0')
  return `${hour}:${minute}`
}

function toIsoDate(date: Date): string {
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}
</script>

<template>
  <div class="modal-backdrop" role="presentation" @click="onBackdropClick">
    <section
      ref="modalCardRef"
      class="modal-card modal-card--appointment"
      role="dialog"
      aria-modal="true"
      aria-labelledby="consultation-modal-title"
      @click="onDialogClick"
    >
      <header class="appointment-modal__header">
        <div>
          <p class="eyebrow">Request appointment</p>
          <h2 id="consultation-modal-title">Schedule consultation</h2>
          <p class="muted">Choose a time and share brief details so we can prepare for your consultation.</p>
        </div>
        <button type="button" class="appointment-modal__close" aria-label="Close" @click="closeModal">×</button>
      </header>

      <form class="appointment-modal__form" @submit.prevent="submitConsultation">
        <div class="appointment-modal__details">
          <div class="appointment-modal__fields">
            <label for="consultation-full-name">
              Full name
              <input id="consultation-full-name" ref="firstInputRef" v-model="fullName" type="text" required />
            </label>
            <label for="consultation-email">
              Email
              <input id="consultation-email" v-model="email" type="email" required />
            </label>
            <label for="consultation-phone">
              Phone
              <input id="consultation-phone" v-model="phone" type="tel" placeholder="Optional" />
            </label>
            <label for="consultation-practice-area">
              Practice area
              <select id="consultation-practice-area" v-model="practiceArea">
                <option v-for="option in practiceAreas" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </label>
            <label for="consultation-message" class="appointment-modal__message">
              Message
              <textarea
                id="consultation-message"
                v-model="message"
                rows="2"
                placeholder="Share context that will help us prepare."
              />
            </label>
          </div>

          <p v-if="submitError" class="error-message">{{ submitError }}</p>
          <p v-if="submitSuccess" class="success-message">{{ submitSuccess }}</p>

          <div class="modal-actions">
            <button type="button" class="mini-action" @click="closeModal" :disabled="isSubmitting">Cancel</button>
            <button
              type="submit"
              class="hero-button hero-button--primary"
              :disabled="isSubmitting || isLoadingSlots || calendarDays.length === 0 || !activeSelectedSlot"
            >
              {{ isSubmitting ? 'Sending...' : 'Send request' }}
            </button>
          </div>
        </div>

        <fieldset class="consultation-picker appointment-modal__schedule" :disabled="isLoadingSlots && slots.length === 0">
          <legend>Available appointment time</legend>
          <div class="appointment-modal__schedule-body">
            <template v-if="calendarDays.length > 0">
              <div class="consultation-picker__calendar">
                <div class="consultation-picker__calendar-header">
                  <button
                    type="button"
                    class="consultation-picker__month-nav"
                    :disabled="!canGoToPreviousMonth"
                    aria-label="Previous month"
                    @click="canGoToPreviousMonth && moveMonth(-1)"
                  >
                    ←
                  </button>
                  <p class="consultation-picker__month-label">
                    {{ visibleMonthStart.toLocaleDateString([], { month: 'long', year: 'numeric' }) }}
                  </p>
                  <button type="button" class="consultation-picker__month-nav" aria-label="Next month" @click="moveMonth(1)">
                    →
                  </button>
                </div>
                <p class="consultation-picker__hint">Choose a date first. Dates with no open times are shown as unavailable.</p>
                <div class="consultation-picker__weekdays" aria-hidden="true">
                  <span v-for="weekday in ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']" :key="weekday">{{ weekday }}</span>
                </div>
                <div class="consultation-picker__calendar-grid" role="radiogroup" aria-label="Appointment dates">
                  <button
                    v-for="(dayItem, index) in calendarDays"
                    :key="dayItem.dayKey"
                    type="button"
                    :class="[
                      'consultation-picker__day',
                      activeDayKey === dayItem.dayKey && 'consultation-picker__day--selected',
                      !dayItem.hasAvailability && 'consultation-picker__day--unavailable',
                      dayItem.isPastDate && 'consultation-picker__day--past',
                      !dayItem.inCurrentMonth && 'consultation-picker__day--outside-month',
                    ]"
                    :disabled="!dayItem.hasAvailability"
                    role="radio"
                    :aria-checked="activeDayKey === dayItem.dayKey"
                    :aria-label="`${dayItem.date.toLocaleDateString([], { weekday: 'long', month: 'long', day: 'numeric' })}${dayItem.isPastDate ? ', unavailable, past date' : !dayItem.hasAvailability ? ', unavailable' : ', available'}`"
                    @keydown="onCalendarDayKeydown($event, index)"
                    @click="
                      selectedDayKey = dayItem.dayKey;
                      selectedSlot = availableSlotMapByDay.get(dayItem.dayKey)?.values().next().value ?? '';
                    "
                  >
                    <span>{{ dayItem.date.getDate() }}</span>
                  </button>
                </div>
              </div>

              <div class="consultation-picker__time-list" role="radiogroup" aria-label="Available times">
                <button
                  v-for="(slot, index) in selectedDaySlots"
                  :key="slot.timeKey"
                  type="button"
                  :class="[
                    'consultation-picker__time',
                    activeSelectedSlot === slot.availableUtc && 'consultation-picker__time--selected',
                    !slot.availableUtc && 'consultation-picker__time--unavailable',
                  ]"
                  :disabled="!slot.availableUtc"
                  role="radio"
                  :aria-checked="activeSelectedSlot === slot.availableUtc"
                  :aria-label="`${slot.label}${!slot.availableUtc ? ', unavailable' : ', available'}`"
                  @keydown="onTimeKeydown($event, index)"
                  @click="slot.availableUtc && (selectedSlot = slot.availableUtc)"
                >
                  {{ slot.label }}
                </button>
              </div>

              <p class="consultation-picker__selection-summary" aria-live="polite">
                {{
                  activeSelectedSlot
                    ? `Selected: ${new Date(activeSelectedSlot).toLocaleDateString([], {
                        weekday: 'long',
                        month: 'long',
                        day: 'numeric',
                      })} at ${new Date(activeSelectedSlot).toLocaleTimeString([], {
                        hour: 'numeric',
                        minute: '2-digit',
                      })}`
                    : 'No time selected yet.'
                }}
              </p>
            </template>

            <p v-else-if="!isLoadingSlots" class="muted">No open slots right now.</p>
          </div>
        </fieldset>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { getConsultationQueue, updateConsultationStatus } from '../../features/consultations/api'
import type { ConsultationRequestItem } from '../../features/consultations/types'

const auth = useAuthSession()
const records = ref<ConsultationRequestItem[]>([])
const statusFilter = ref('')
const searchTerm = ref('')
const isLoading = ref(false)
const isSavingId = ref<string | null>(null)
const error = ref<string | null>(null)
const drafts = ref<Record<string, { status: string; internalNotes: string }>>({})

const queueStatuses = ['Requested', 'InReview', 'Contacted', 'Scheduled', 'Converted', 'Closed', 'Cancelled']

const statusCounts = computed(() => {
  const counts = Object.fromEntries(queueStatuses.map((status) => [status, 0])) as Record<string, number>
  for (const item of records.value) {
    counts[item.status] = (counts[item.status] ?? 0) + 1
  }
  return counts
})

const visibleRecords = computed(() => {
  const query = searchTerm.value.trim().toLowerCase()
  if (!query) {
    return records.value
  }

  return records.value.filter((item) => {
    const assigned = item.assignedToDisplayName ?? ''
    const notes = drafts.value[item.id]?.internalNotes ?? item.internalNotes ?? ''
    return [item.fullName, item.email, item.phone ?? '', item.practiceArea, item.status, assigned, notes]
      .join(' ')
      .toLowerCase()
      .includes(query)
  })
})

async function loadQueue() {
  const token = auth.token.value
  if (!token) return

  isLoading.value = true
  error.value = null
  try {
    records.value = await getConsultationQueue(token, statusFilter.value || undefined)
    drafts.value = Object.fromEntries(
      records.value.map((item) => [item.id, { status: item.status, internalNotes: item.internalNotes ?? '' }]),
    )
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load consultation queue.'
  } finally {
    isLoading.value = false
  }
}

async function saveItem(item: ConsultationRequestItem) {
  const token = auth.token.value
  const actor = auth.actor.value
  if (!token || !actor) {
    return
  }

  const draft = drafts.value[item.id]
  if (!draft) {
    return
  }

  isSavingId.value = item.id
  error.value = null
  try {
    await updateConsultationStatus(token, item.id, {
      status: draft.status,
      assignedToActorId: `user-${actor.id}`,
      assignedToDisplayName: actor.name,
      internalNotes: draft.internalNotes || undefined,
    })
    await loadQueue()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to update consultation status.'
  } finally {
    isSavingId.value = null
  }
}

function updateDraftStatus(item: ConsultationRequestItem, event: Event) {
  const target = event.target as HTMLSelectElement | null
  if (!target) {
    return
  }

  drafts.value[item.id] = {
    status: target.value,
    internalNotes: drafts.value[item.id]?.internalNotes ?? '',
  }
}

function updateDraftNotes(item: ConsultationRequestItem, event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement | null
  if (!target) {
    return
  }

  drafts.value[item.id] = {
    status: drafts.value[item.id]?.status ?? item.status,
    internalNotes: target.value,
  }
}

function formatDateTime(value: string): string {
  return new Date(value).toLocaleString()
}

function statusTone(status: string): string {
  switch (status) {
    case 'Requested':
      return 'consultation-status--requested'
    case 'InReview':
      return 'consultation-status--review'
    case 'Contacted':
      return 'consultation-status--contacted'
    case 'Scheduled':
      return 'consultation-status--scheduled'
    case 'Converted':
      return 'consultation-status--converted'
    case 'Closed':
      return 'consultation-status--closed'
    case 'Cancelled':
      return 'consultation-status--cancelled'
    default:
      return ''
  }
}

onMounted(() => {
  void loadQueue()
})
</script>

<template>
  <section class="workspace-page consultations-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Consultations</p>
      <h1>Consultation follow-up queue</h1>
      <p class="muted">
        Public booking requests from the contact modal appear here for legal assistant and attorney follow-through.
      </p>
    </header>

    <article class="card consultations-stats">
      <div class="consultations-stats__items">
        <div v-for="status in queueStatuses" :key="status" class="consultations-stats__item">
          <p class="consultations-stats__label">{{ status }}</p>
          <p class="consultations-stats__value">{{ statusCounts[status] ?? 0 }}</p>
        </div>
      </div>
    </article>

    <article class="card consultations-toolbar">
      <div class="consultations-toolbar__controls">
        <label class="workspace-form__inline consultations-toolbar__control">
          Queue status
          <select v-model="statusFilter" @change="loadQueue">
            <option value="">All statuses</option>
            <option v-for="status in queueStatuses" :key="status" :value="status">{{ status }}</option>
          </select>
        </label>

        <label class="workspace-form__inline consultations-toolbar__control">
          Search
          <input v-model="searchTerm" type="search" placeholder="Name, email, status, notes..." />
        </label>
      </div>
      <button type="button" class="hero-button" @click="loadQueue">Reload queue</button>
    </article>

    <p v-if="isLoading" class="muted">Loading consultation queue...</p>
    <p v-else-if="visibleRecords.length === 0" class="card muted consultations-empty">
      No consultation requests match the current filters.
    </p>
    <ul v-else class="consultation-queue">
      <li v-for="item in visibleRecords" :key="item.id" class="consultation-item">
        <div class="consultation-item__header">
          <div>
            <strong>{{ item.fullName }}</strong>
            <p class="muted">
              {{ item.practiceArea }} · {{ item.email }} · {{ item.phone ?? 'No phone' }}
            </p>
          </div>
          <span class="consultation-status" :class="statusTone(drafts[item.id]?.status ?? item.status)">
            {{ drafts[item.id]?.status ?? item.status }}
          </span>
        </div>

        <div class="consultation-item__meta">
          <p><span>Preferred:</span> {{ formatDateTime(item.preferredAtUtc) }} ({{ item.timeZone }})</p>
          <p><span>Submitted:</span> {{ formatDateTime(item.createdAtUtc) }}</p>
          <p v-if="item.message"><span>Client note:</span> {{ item.message }}</p>
        </div>

        <div class="consultation-item__editor">
          <label class="workspace-form__inline">
            Status
            <select
              :value="drafts[item.id]?.status ?? item.status"
              :disabled="!auth.hasPermission('consultations.manage') || isSavingId === item.id"
              @change="updateDraftStatus(item, $event)"
            >
              <option v-for="status in queueStatuses" :key="status" :value="status">{{ status }}</option>
            </select>
          </label>

          <label class="workspace-form__inline consultation-item__notes">
            Internal notes
            <textarea
              :value="drafts[item.id]?.internalNotes ?? ''"
              :disabled="!auth.hasPermission('consultations.manage') || isSavingId === item.id"
              placeholder="Capture triage notes, priority, assignment context..."
              @input="updateDraftNotes(item, $event)"
            />
          </label>

          <button
            type="button"
            class="hero-button hero-button--primary consultation-item__save"
            :disabled="isSavingId === item.id || !auth.hasPermission('consultations.manage')"
            @click="saveItem(item)"
          >
            {{ isSavingId === item.id ? 'Saving...' : 'Save update' }}
          </button>
        </div>
      </li>
    </ul>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { getConsultationQueue, updateConsultationStatus } from '../../features/consultations/api'
import type { ConsultationRequestItem } from '../../features/consultations/types'

const auth = useAuthSession()
const records = ref<ConsultationRequestItem[]>([])
const statusFilter = ref('')
const isLoading = ref(false)
const isSavingId = ref<string | null>(null)
const error = ref<string | null>(null)
const drafts = ref<Record<string, { status: string; internalNotes: string }>>({})

const queueStatuses = ['Requested', 'InReview', 'Contacted', 'Scheduled', 'Converted', 'Closed', 'Cancelled']

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
  const target = event.target as HTMLInputElement | null
  if (!target) {
    return
  }

  drafts.value[item.id] = {
    status: drafts.value[item.id]?.status ?? item.status,
    internalNotes: target.value,
  }
}

onMounted(() => {
  void loadQueue()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Consultations</p>
      <h1>Consultation follow-up queue</h1>
      <p class="muted">
        Public booking requests from the contact modal appear here for legal assistant and attorney follow-through.
      </p>
    </header>

    <article class="card">
      <div class="workspace-actions">
        <label class="workspace-form__inline">
          Status
          <select v-model="statusFilter" @change="loadQueue">
            <option value="">All statuses</option>
            <option v-for="status in queueStatuses" :key="status" :value="status">{{ status }}</option>
          </select>
        </label>
        <button type="button" class="hero-button" @click="loadQueue">Reload queue</button>
      </div>

      <p v-if="isLoading" class="muted">Loading consultation queue...</p>
      <ul v-else class="record-list">
        <li v-for="item in records" :key="item.id">
          <strong>{{ item.fullName }} ({{ item.status }})</strong>
          <span>{{ item.practiceArea }} | {{ item.email }} | {{ item.phone ?? 'No phone' }}</span>
          <span>Preferred: {{ new Date(item.preferredAtUtc).toLocaleString() }} ({{ item.timeZone }})</span>
          <span>Submitted: {{ new Date(item.createdAtUtc).toLocaleString() }}</span>
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
          <label class="workspace-form__inline">
            Internal notes
            <input
              :value="drafts[item.id]?.internalNotes ?? ''"
              :disabled="!auth.hasPermission('consultations.manage') || isSavingId === item.id"
              @input="updateDraftNotes(item, $event)"
            />
          </label>
          <button
            type="button"
            class="hero-button hero-button--primary"
            :disabled="isSavingId === item.id || !auth.hasPermission('consultations.manage')"
            @click="saveItem(item)"
          >
            {{ isSavingId === item.id ? 'Saving...' : 'Save update' }}
          </button>
        </li>
      </ul>
    </article>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

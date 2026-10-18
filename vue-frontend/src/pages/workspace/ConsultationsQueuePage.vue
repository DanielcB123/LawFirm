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
const isSaving = ref(false)
const error = ref<string | null>(null)

const queueStatuses = ['Requested', 'Scheduled', 'InReview', 'Converted', 'Closed']

async function loadQueue() {
  const token = auth.token.value
  if (!token) return

  isLoading.value = true
  error.value = null
  try {
    records.value = await getConsultationQueue(token, statusFilter.value || undefined)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load consultation queue.'
  } finally {
    isLoading.value = false
  }
}

async function markScheduled(item: ConsultationRequestItem) {
  const token = auth.token.value
  const actor = auth.actor.value
  if (!token || !actor) return

  isSaving.value = true
  error.value = null
  try {
    await updateConsultationStatus(token, item.id, {
      status: 'Scheduled',
      assignedToActorId: `user-${actor.id}`,
      assignedToDisplayName: actor.name,
      internalNotes: item.internalNotes ?? 'Follow-up call scheduled.',
    })
    await loadQueue()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to update consultation status.'
  } finally {
    isSaving.value = false
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
          <span>Preferred: {{ new Date(item.preferredAtUtc).toLocaleString() }}</span>
          <button
            type="button"
            class="hero-button hero-button--primary"
            :disabled="isSaving || !auth.hasPermission('consultations.manage')"
            @click="markScheduled(item)"
          >
            Mark scheduled
          </button>
        </li>
      </ul>
    </article>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

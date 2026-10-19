<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { getMatterList } from '../../features/matters/api'
import type { MatterListItem } from '../../features/matters/types'

const auth = useAuthSession()
const matters = ref<MatterListItem[]>([])
const error = ref<string | null>(null)
const isLoading = ref(false)

async function loadMatters() {
  const token = auth.token.value
  if (!token || !auth.hasPermission('matters.read')) {
    matters.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    matters.value = await getMatterList(token)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load matters.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void loadMatters()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Matters</p>
      <h1>Matter workspace index</h1>
      <p class="muted">Open matters, staffing, and stage progress that drive the tasks and calendar queues.</p>
    </header>

    <article class="card">
      <p v-if="!auth.hasPermission('matters.read')" class="muted">
        Current user does not have matters.read permission.
      </p>
      <p v-else-if="isLoading" class="muted">Loading matters...</p>
      <ul v-else class="record-list">
        <li v-for="matter in matters" :key="matter.id">
          <strong>{{ matter.matterNumber }} — {{ matter.title }}</strong>
          <span>{{ matter.practiceArea }} | Stage: {{ matter.stage }} | Status: {{ matter.status }}</span>
          <span>
            Attorney: {{ matter.responsibleAttorneyDisplayName ?? 'Unassigned' }} |
            Paralegal: {{ matter.paralegalDisplayName ?? 'Unassigned' }}
          </span>
          <span>Open tasks: {{ matter.openTaskCount }}</span>
        </li>
      </ul>
    </article>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

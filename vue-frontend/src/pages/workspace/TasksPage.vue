<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { createTask, getMatterList, getTaskList, updateTaskStatus } from '../../features/matters/api'
import type { MatterListItem, MatterTask } from '../../features/matters/types'

const auth = useAuthSession()
const tasks = ref<MatterTask[]>([])
const matters = ref<MatterListItem[]>([])
const statusFilter = ref('')
const error = ref<string | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const form = ref({
  matterId: '',
  title: '',
  priority: 'Normal' as MatterTask['priority'],
  dueAtLocal: '',
})

const canManage = computed(() => auth.hasPermission('tasks.manage'))

async function loadTaskData() {
  const token = auth.token.value
  if (!token || !auth.hasPermission('tasks.read')) {
    tasks.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    const [taskItems, matterItems] = await Promise.all([getTaskList(token, statusFilter.value || undefined), getMatterList(token)])
    tasks.value = taskItems
    matters.value = matterItems
    if (!form.value.matterId && matterItems.length > 0) {
      form.value.matterId = matterItems[0].id
    }
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load tasks.'
  } finally {
    isLoading.value = false
  }
}

async function submitTask() {
  const token = auth.token.value
  const actor = auth.actor.value
  if (!token || !actor || !canManage.value || !form.value.matterId) {
    return
  }

  isSaving.value = true
  error.value = null
  try {
    await createTask(token, {
      matterId: form.value.matterId,
      title: form.value.title.trim(),
      priority: form.value.priority,
      assigneeActorId: `user-${actor.id}`,
      assigneeDisplayName: actor.name,
      dueAtUtc: form.value.dueAtLocal ? new Date(form.value.dueAtLocal).toISOString() : undefined,
    })
    form.value.title = ''
    await loadTaskData()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create task.'
  } finally {
    isSaving.value = false
  }
}

async function setStatus(task: MatterTask, status: MatterTask['status']) {
  const token = auth.token.value
  if (!token || !canManage.value) {
    return
  }

  try {
    await updateTaskStatus(token, task.id, status)
    await loadTaskData()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to update task.'
  }
}

onMounted(() => {
  void loadTaskData()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Tasks</p>
      <h1>Task queue</h1>
      <p class="muted">Track task progress across matters and move items from open work to completion.</p>
    </header>

    <article class="card">
      <div class="workspace-actions">
        <label class="workspace-form__inline">
          Status
          <select v-model="statusFilter" @change="loadTaskData">
            <option value="">All statuses</option>
            <option value="Open">Open</option>
            <option value="InProgress">InProgress</option>
            <option value="Waiting">Waiting</option>
            <option value="Completed">Completed</option>
          </select>
        </label>
        <button type="button" class="hero-button" @click="loadTaskData">Reload tasks</button>
      </div>

      <p v-if="!auth.hasPermission('tasks.read')" class="muted">Current user does not have tasks.read permission.</p>
      <p v-else-if="isLoading" class="muted">Loading tasks...</p>
      <ul v-else class="record-list">
        <li v-for="task in tasks" :key="task.id">
          <strong>{{ task.title }} ({{ task.status }})</strong>
          <span>{{ task.matterNumber ?? 'No matter' }} | {{ task.matterTitle ?? 'Unassigned matter title' }}</span>
          <span>
            Priority: {{ task.priority }} | Due:
            {{ task.dueAtUtc ? new Date(task.dueAtUtc).toLocaleString() : 'No due date' }}
          </span>
          <div class="workspace-actions">
            <button type="button" class="mini-action" :disabled="!canManage" @click="setStatus(task, 'InProgress')">Start</button>
            <button type="button" class="mini-action" :disabled="!canManage" @click="setStatus(task, 'Waiting')">Waiting</button>
            <button type="button" class="mini-action" :disabled="!canManage" @click="setStatus(task, 'Completed')">Complete</button>
          </div>
        </li>
      </ul>
    </article>

    <article class="card workspace-form-card">
      <h2>Create matter task</h2>
      <form class="workspace-form" @submit.prevent="submitTask">
        <label>
          Matter
          <select v-model="form.matterId" required>
            <option v-for="matter in matters" :key="matter.id" :value="matter.id">
              {{ matter.matterNumber }} — {{ matter.title }}
            </option>
          </select>
        </label>
        <label>
          Title
          <input v-model="form.title" type="text" required />
        </label>
        <label>
          Priority
          <select v-model="form.priority">
            <option value="Low">Low</option>
            <option value="Normal">Normal</option>
            <option value="High">High</option>
            <option value="Urgent">Urgent</option>
          </select>
        </label>
        <label>
          Due at
          <input v-model="form.dueAtLocal" type="datetime-local" />
        </label>
        <button type="submit" :disabled="isSaving || !canManage">
          {{ isSaving ? 'Saving...' : 'Create task' }}
        </button>
      </form>
    </article>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

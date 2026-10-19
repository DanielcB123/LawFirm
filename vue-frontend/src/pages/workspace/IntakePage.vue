<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ApiClientError, getJson, postJson } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'

type IntakeRecord = {
  id: string
  prospectiveClientName: string
  practiceArea: string
  intakeType: string
  stage: string
}

const auth = useAuthSession()
const records = ref<IntakeRecord[]>([])
const error = ref<string | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const form = ref({
  prospectiveClientName: '',
  practiceArea: 'ConstructionLaw',
  intakeType: 'Website',
  nextAction: '',
})

async function loadIntake() {
  const token = auth.token.value
  if (!token || !auth.hasPermission('intake.read')) {
    records.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    records.value = await getJson<IntakeRecord[]>('/intake', token)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load intake queue.'
  } finally {
    isLoading.value = false
  }
}

async function createIntake() {
  const token = auth.token.value
  const actor = auth.actor.value
  if (!token || !actor) return

  isSaving.value = true
  error.value = null
  try {
    await postJson(
      '/intake',
      {
        prospectiveClientName: form.value.prospectiveClientName.trim(),
        practiceArea: form.value.practiceArea,
        intakeType: form.value.intakeType,
        ownerActorId: `user-${actor.id}`,
        ownerDisplayName: actor.name,
        nextAction: form.value.nextAction || null,
      },
      token,
    )
    form.value = {
      prospectiveClientName: '',
      practiceArea: 'ConstructionLaw',
      intakeType: 'Website',
      nextAction: '',
    }
    await loadIntake()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create intake record.'
  } finally {
    isSaving.value = false
  }
}

onMounted(() => {
  void loadIntake()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Intake</p>
      <h1>Intake queue</h1>
    </header>

    <div class="content-grid">
      <article class="card workspace-form-card">
        <h2>Create intake record</h2>
        <form class="workspace-form" @submit.prevent="createIntake">
          <label>
            Prospective client name
            <input v-model="form.prospectiveClientName" type="text" required />
          </label>
          <label>
            Practice area
            <input v-model="form.practiceArea" type="text" />
          </label>
          <label>
            Intake type
            <input v-model="form.intakeType" type="text" />
          </label>
          <label>
            Next action
            <input v-model="form.nextAction" type="text" />
          </label>
          <button type="submit" :disabled="isSaving || !auth.hasPermission('intake.manage')">
            {{ isSaving ? 'Saving...' : 'Create intake' }}
          </button>
        </form>
      </article>

      <article class="card">
        <h2>Queue</h2>
        <p v-if="!auth.hasPermission('intake.read')" class="muted">
          Current user does not have intake.read permission.
        </p>
        <p v-else-if="isLoading" class="muted">Loading intake records...</p>
        <ul v-else class="record-list">
          <li v-for="item in records" :key="item.id">
            <strong>{{ item.prospectiveClientName }}</strong>
            <span>{{ item.stage }}</span>
            <span>{{ item.practiceArea }}</span>
            <span>{{ item.intakeType }}</span>
          </li>
        </ul>
      </article>
    </div>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

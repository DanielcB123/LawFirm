<script setup lang="ts">
import { onMounted, ref } from 'vue'

type HealthResponse = {
  status: string
  service: string
  database: string
}

type Contact = {
  id: number
  display_name: string
  contact_type: 'Person' | 'Organization'
  primary_email: string | null
  primary_phone: string | null
}

type IntakeRecord = {
  id: string
  prospectiveClientName: string
  practiceArea: string
  intakeType: string
  stage: string
  ownerActorId: string
  ownerDisplayName: string
  nextAction: string | null
  createdAtUtc: string | null
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080/api'
const health = ref<HealthResponse | null>(null)
const error = ref<string | null>(null)
const isLoading = ref(false)
const contacts = ref<Contact[]>([])
const intakeRecords = ref<IntakeRecord[]>([])
const isSaving = ref(false)
const isSigningIn = ref(false)
const isSavingIntake = ref(false)
const token = ref<string | null>(null)
const actor = ref<{ id: number; name: string; email: string; role: string; permissions: string[] } | null>(null)
const query = ref('')
const loginForm = ref({
  email: 'attorney@matterforge.local',
  password: 'Passw0rd!',
})
const form = ref({
  display_name: '',
  contact_type: 'Person' as 'Person' | 'Organization',
  primary_email: '',
  primary_phone: '',
})
const intakeForm = ref({
  prospectiveClientName: '',
  practiceArea: 'ConstructionLaw',
  intakeType: 'Website',
  nextAction: '',
})
const quickStats = [
  { label: 'Avg. response window', value: '< 1 business day' },
  { label: 'Practice model', value: 'Client-first, trial-ready' },
  { label: 'Consultations', value: 'Same-week availability' },
]

function hasPermission(permission: string): boolean {
  return !!actor.value?.permissions?.includes(permission)
}

async function loadHealth() {
  isLoading.value = true
  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/health`, {
      headers: { Accept: 'application/json' },
    })

    if (!response.ok) {
      throw new Error(`Health request failed (${response.status})`)
    }

    health.value = (await response.json()) as HealthResponse
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Health request failed'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void loadHealth()
})

async function loadContacts() {
  if (!token.value) {
    contacts.value = []
    return
  }

  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/contacts?q=${encodeURIComponent(query.value)}`, {
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${token.value}`,
      },
    })

    if (!response.ok) {
      throw new Error(`Contacts request failed (${response.status})`)
    }

    contacts.value = (await response.json()) as Contact[]
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Contacts request failed'
  }
}

async function loadIntakeRecords() {
  if (!token.value || !hasPermission('intake.read')) {
    intakeRecords.value = []
    return
  }

  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/intake`, {
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${token.value}`,
      },
    })

    if (!response.ok) {
      throw new Error(`Intake request failed (${response.status})`)
    }

    intakeRecords.value = (await response.json()) as IntakeRecord[]
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Intake request failed'
  }
}

async function createContact() {
  if (!token.value) {
    error.value = 'Sign in is required before creating contacts.'
    return
  }

  if (!form.value.display_name.trim()) {
    error.value = 'Display name is required.'
    return
  }

  isSaving.value = true
  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/contacts`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token.value}`,
      },
      body: JSON.stringify({
        display_name: form.value.display_name.trim(),
        contact_type: form.value.contact_type,
        primary_email: form.value.primary_email || null,
        primary_phone: form.value.primary_phone || null,
      }),
    })

    if (!response.ok) {
      throw new Error(`Create contact failed (${response.status})`)
    }

    form.value = {
      display_name: '',
      contact_type: 'Person',
      primary_email: '',
      primary_phone: '',
    }
    await loadContacts()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Create contact failed'
  } finally {
    isSaving.value = false
  }
}

async function createIntake() {
  if (!token.value) {
    error.value = 'Sign in is required before creating intake records.'
    return
  }

  if (!hasPermission('intake.manage')) {
    error.value = 'Your account does not have intake.manage permission.'
    return
  }

  if (!intakeForm.value.prospectiveClientName.trim()) {
    error.value = 'Prospective client name is required.'
    return
  }

  isSavingIntake.value = true
  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/intake`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token.value}`,
      },
      body: JSON.stringify({
        prospectiveClientName: intakeForm.value.prospectiveClientName.trim(),
        practiceArea: intakeForm.value.practiceArea,
        intakeType: intakeForm.value.intakeType,
        ownerActorId: `user-${actor.value?.id ?? 'unknown'}`,
        ownerDisplayName: actor.value?.name ?? 'Unknown User',
        nextAction: intakeForm.value.nextAction || null,
      }),
    })

    if (!response.ok) {
      throw new Error(`Create intake failed (${response.status})`)
    }

    intakeForm.value = {
      prospectiveClientName: '',
      practiceArea: 'ConstructionLaw',
      intakeType: 'Website',
      nextAction: '',
    }
    await loadIntakeRecords()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Create intake failed'
  } finally {
    isSavingIntake.value = false
  }
}

async function signIn() {
  isSigningIn.value = true
  error.value = null

  try {
    const response = await fetch(`${apiBaseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: loginForm.value.email.trim(),
        password: loginForm.value.password,
      }),
    })

    if (!response.ok) {
      throw new Error(`Sign in failed (${response.status})`)
    }

    const payload = (await response.json()) as {
      access_token: string
      user: { id: number; name: string; email: string; role: string; permissions: string[] }
    }

    token.value = payload.access_token
    actor.value = payload.user
    await loadContacts()
    await loadIntakeRecords()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Sign in failed'
  } finally {
    isSigningIn.value = false
  }
}
</script>

<template>
  <div class="site-header">
    <div class="site-header__inner">
      <p class="site-brand">Brian & Sandra Law</p>
      <p class="site-header__meta">MatterForge migration workspace</p>
    </div>
  </div>

  <main class="page home-page">
    <section class="home-hero">
      <div class="home-hero__image" aria-hidden="true"></div>
      <div class="home-hero__glow" aria-hidden="true"></div>
      <div class="home-hero__grid" aria-hidden="true"></div>
      <p class="eyebrow home-hero__eyebrow">Trusted legal counsel in Ohio</p>
      <h1>Legal strategy that moves as fast as your life.</h1>
      <p class="lead">
        Vue + Laravel migration environment with production-style design, while preserving your new authenticated contacts and intake workflows.
      </p>
      <div class="home-hero__actions">
        <button type="button" class="hero-button hero-button--primary" @click="loadHealth" :disabled="isLoading">
          {{ isLoading ? 'Checking API health...' : 'Check API health' }}
        </button>
        <button type="button" class="hero-button" @click="loadContacts" :disabled="!token">Reload contacts</button>
        <button type="button" class="hero-button" @click="loadIntakeRecords" :disabled="!token || !hasPermission('intake.read')">
          Reload intake
        </button>
      </div>
      <div class="home-hero__stats" aria-label="Firm highlights">
        <article class="card home-stat" v-for="stat in quickStats" :key="stat.label">
          <p class="home-stat__label">{{ stat.label }}</p>
          <p class="home-stat__value">{{ stat.value }}</p>
        </article>
      </div>
    </section>

    <section class="content-grid">
      <article class="card home-panel home-panel--lift workspace-form-card">
        <p class="eyebrow">Authentication</p>
        <h2>Sign in</h2>
        <form class="workspace-form" @submit.prevent="signIn">
          <label>
            Email
            <input v-model="loginForm.email" type="email" />
          </label>
          <label>
            Password
            <input v-model="loginForm.password" type="password" />
          </label>
          <button type="submit" :disabled="isSigningIn">
            {{ isSigningIn ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>
        <div v-if="actor" class="identity-card">
          <p><strong>Signed in as:</strong> {{ actor.name }} ({{ actor.email }})</p>
          <p><strong>Role:</strong> {{ actor.role }}</p>
          <p><strong>Permissions:</strong> {{ actor.permissions.join(', ') }}</p>
        </div>
      </article>

      <article class="card home-panel home-panel--lift api-card">
        <p class="eyebrow">Runtime status</p>
        <h2>API connection</h2>
        <p class="api-base"><strong>API base URL:</strong> {{ apiBaseUrl }}</p>
        <pre v-if="health" class="health-json">{{ health }}</pre>
        <p v-else class="muted">Run "Check API health" to load status details.</p>
      </article>
    </section>

    <section class="content-grid">
      <article class="card home-panel home-panel--lift workspace-form-card">
        <p class="eyebrow">Contacts</p>
        <h2>Create contact</h2>
        <form class="workspace-form" @submit.prevent="createContact">
          <label>
            Name
            <input v-model="form.display_name" type="text" />
          </label>
          <label>
            Type
            <select v-model="form.contact_type">
              <option value="Person">Person</option>
              <option value="Organization">Organization</option>
            </select>
          </label>
          <label>
            Email
            <input v-model="form.primary_email" type="email" />
          </label>
          <label>
            Phone
            <input v-model="form.primary_phone" type="text" />
          </label>
          <button type="submit" :disabled="isSaving || !token">
            {{ isSaving ? 'Saving...' : 'Create contact' }}
          </button>
        </form>
      </article>

      <article class="card home-panel home-panel--lift">
        <p class="eyebrow">Directory</p>
        <h2>Contacts</h2>
        <label class="workspace-form__inline">
          Search
          <input v-model="query" type="text" @change="loadContacts" />
        </label>
        <ul class="record-list">
          <li v-for="contact in contacts" :key="contact.id">
            <strong>{{ contact.display_name }}</strong>
            <span>{{ contact.contact_type }}</span>
            <span>{{ contact.primary_email ?? 'No email on file' }}</span>
            <span>{{ contact.primary_phone ?? 'No phone on file' }}</span>
          </li>
        </ul>
      </article>
    </section>

    <section class="content-grid">
      <article class="card home-panel home-panel--lift workspace-form-card">
        <p class="eyebrow">Intake workflow</p>
        <h2>Create intake record</h2>
        <form class="workspace-form" @submit.prevent="createIntake">
          <label>
            Prospective client name
            <input v-model="intakeForm.prospectiveClientName" type="text" />
          </label>
          <label>
            Practice area
            <input v-model="intakeForm.practiceArea" type="text" />
          </label>
          <label>
            Intake type
            <input v-model="intakeForm.intakeType" type="text" />
          </label>
          <label>
            Next action
            <input v-model="intakeForm.nextAction" type="text" />
          </label>
          <button type="submit" :disabled="isSavingIntake || !token || !hasPermission('intake.manage')">
            {{ isSavingIntake ? 'Saving...' : 'Create intake' }}
          </button>
        </form>
      </article>

      <article class="card home-panel home-panel--lift">
        <p class="eyebrow">Pipeline</p>
        <h2>Intake records</h2>
        <p v-if="!hasPermission('intake.read')" class="muted">
          Current user does not have intake.read permission.
        </p>
        <ul class="record-list" v-else>
          <li v-for="item in intakeRecords" :key="item.id">
            <strong>{{ item.prospectiveClientName }}</strong>
            <span>{{ item.stage }}</span>
            <span>{{ item.practiceArea }}</span>
            <span>{{ item.intakeType }}</span>
          </li>
        </ul>
      </article>
    </section>

    <p v-if="error" class="error-message">{{ error }}</p>
  </main>

  <footer class="site-footer">
    <div class="site-footer__inner">
      <p>Brian & Sandra Law | Client-focused representation for families and businesses.</p>
      <p>123 Justice Avenue, Suite 400, Columbus, OH 43215 | (614) 555-0134</p>
      <p>{{ new Date().getFullYear() }} Brian & Sandra Law. All rights reserved.</p>
    </div>
  </footer>
</template>

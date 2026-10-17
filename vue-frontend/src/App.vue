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

const valueProps = [
  'Responsive communication and transparent billing.',
  'Focused experience in high-stakes civil matters.',
  'Personalized strategy tailored to your goals.',
]

const practiceAreaHighlights = [
  {
    title: 'Business & Contract Disputes',
    body: 'Strategic negotiation and litigation support to protect your company, partnerships, and future growth.',
  },
  {
    title: 'Risk & Compliance Advisory',
    body: 'Proactive legal guidance to reduce exposure, improve policy discipline, and prevent costly disputes.',
  },
  {
    title: 'Civil Litigation',
    body: 'End-to-end courtroom representation from early case strategy through trial and post-judgment enforcement.',
  },
  {
    title: 'Estate & Legacy Planning',
    body: 'Wills, trusts, and planning frameworks that bring clarity and confidence to major life transitions.',
  },
]

const partnerProfiles = [
  {
    name: 'Sandra Whitmore',
    role: 'Managing Partner - Litigation',
    shortBio:
      'Sandra leads high-stakes litigation strategy with a direct, human approach that keeps clients informed and in control.',
    image:
      'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=1000&q=80',
  },
  {
    name: 'Brian Whitmore',
    role: 'Founding Partner - Counsel',
    shortBio:
      'Brian focuses on early risk analysis, decisive legal positioning, and practical solutions that hold up in court.',
    image:
      'https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=1000&q=80',
  },
]

const processSteps = [
  'Initial consultation to understand your goals, timeline, and constraints.',
  'Case roadmap with clear options, likely outcomes, and next actions.',
  'Execution phase with active communication and transparent milestones.',
  'Resolution and long-term planning to protect what comes next.',
]

const testimonials = [
  {
    quote:
      'They gave us a strategy we could actually follow. Every next step was clear, and we always knew where we stood.',
    source: 'Client - Small business owner',
  },
  {
    quote:
      'Professional, responsive, and honest about options. The process felt manageable because their team stayed ahead of every detail.',
    source: 'Client - Civil litigation matter',
  },
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
      <nav class="site-nav" aria-label="Primary">
        <a class="site-nav__link site-nav__link--active" href="/">Home</a>
        <a class="site-nav__link" href="#about">About</a>
        <a class="site-nav__link" href="#practice">Practice Areas</a>
        <a class="site-nav__link" href="#attorneys">Attorneys</a>
        <a class="site-nav__link" href="#contact">Contact</a>
        <a class="site-nav__link" href="#workspace-tools">Sign in</a>
      </nav>
    </div>
  </div>

  <main class="page home-page app-content">
    <section class="home-hero">
      <div class="home-hero__image" aria-hidden="true"></div>
      <div class="home-hero__glow" aria-hidden="true"></div>
      <div class="home-hero__grid" aria-hidden="true"></div>
      <p class="eyebrow home-hero__eyebrow">Trusted legal counsel in Ohio</p>
      <h1>Legal strategy that moves as fast as your life.</h1>
      <p class="lead">
        Brian & Sandra Law helps individuals, families, and small businesses resolve legal issues with practical strategy,
        consistent communication, and strong courtroom advocacy.
      </p>
      <div class="home-hero__actions">
        <a class="hero-button hero-button--primary" href="#contact">Book consultation</a>
        <a class="hero-button" href="#practice">Explore practice areas</a>
      </div>
      <div class="home-hero__stats" aria-label="Firm highlights">
        <article class="card home-stat" v-for="stat in quickStats" :key="stat.label">
          <p class="home-stat__label">{{ stat.label }}</p>
          <p class="home-stat__value">{{ stat.value }}</p>
        </article>
      </div>
    </section>

    <section id="about" class="content-grid home-panels">
      <article class="card home-panel home-panel--lift">
        <h2>Why clients choose us</h2>
        <ul>
          <li v-for="item in valueProps" :key="item">{{ item }}</li>
        </ul>
      </article>
      <article class="card home-panel home-panel--lift">
        <h2>Consultation hours</h2>
        <p>Monday-Friday: 8:30 AM-6:00 PM</p>
        <p>Saturday: By appointment</p>
        <p>Same-week consultations available for urgent matters.</p>
      </article>
    </section>

    <section id="practice" class="home-practice-grid">
      <article class="card home-practice-card home-panel--lift" v-for="item in practiceAreaHighlights" :key="item.title">
        <p class="home-practice-card__tag">Practice area</p>
        <h3>{{ item.title }}</h3>
        <p>{{ item.body }}</p>
      </article>
    </section>

    <section class="home-parallax-banner" aria-label="Approach">
      <div class="home-parallax-banner__inner">
        <p class="home-parallax-banner__kicker">Approach</p>
        <h2>Built for clarity, speed, and confidence at every legal milestone.</h2>
      </div>
    </section>

    <section id="attorneys" class="home-partners card">
      <p class="home-partners__kicker">Partner spotlight</p>
      <h2>Meet the attorneys behind every strategy.</h2>
      <div class="home-partners__grid">
        <article class="home-partner-card" v-for="partner in partnerProfiles" :key="partner.name">
          <img :src="partner.image" :alt="partner.name" loading="lazy" />
          <div class="home-partner-card__content">
            <h3>{{ partner.name }}</h3>
            <p class="home-partner-card__title">{{ partner.role }}</p>
            <p>{{ partner.shortBio }}</p>
          </div>
        </article>
      </div>
    </section>

    <section class="home-process card">
      <p class="home-process__kicker">What to expect</p>
      <h2>A simple process designed to reduce uncertainty.</h2>
      <ol class="home-process__list">
        <li v-for="step in processSteps" :key="step">{{ step }}</li>
      </ol>
    </section>

    <section class="content-grid home-testimonials">
      <article class="card home-testimonial home-panel--lift" v-for="item in testimonials" :key="item.quote">
        <p class="home-testimonial__quote">"{{ item.quote }}"</p>
        <p class="home-testimonial__source">{{ item.source }}</p>
      </article>
    </section>

    <section id="workspace-tools" class="card workspace-tools">
      <p class="eyebrow">Migration workspace</p>
      <h2>Laravel API tools</h2>
      <p class="muted">
        These tools keep your migrated backend flows testable while preserving the production website aesthetic.
      </p>
      <div class="content-grid workspace-tools__grid">
        <article class="card home-panel home-panel--lift workspace-form-card">
          <h3>Sign in</h3>
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
          <h3>API connection</h3>
          <p class="api-base"><strong>API base URL:</strong> {{ apiBaseUrl }}</p>
          <div class="workspace-actions">
            <button type="button" class="hero-button hero-button--primary" @click="loadHealth" :disabled="isLoading">
              {{ isLoading ? 'Checking API health...' : 'Check API health' }}
            </button>
            <button type="button" class="hero-button" @click="loadContacts" :disabled="!token">Reload contacts</button>
            <button type="button" class="hero-button" @click="loadIntakeRecords" :disabled="!token || !hasPermission('intake.read')">
              Reload intake
            </button>
          </div>
          <pre v-if="health" class="health-json">{{ health }}</pre>
          <p v-else class="muted">Run "Check API health" to load status details.</p>
        </article>

        <article class="card home-panel home-panel--lift workspace-form-card">
          <h3>Create contact</h3>
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
          <h3>Contacts</h3>
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

        <article class="card home-panel home-panel--lift workspace-form-card">
          <h3>Create intake record</h3>
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
          <h3>Intake records</h3>
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
      </div>
    </section>

    <p v-if="error" class="error-message">{{ error }}</p>
  </main>

  <footer id="contact" class="site-footer">
    <div class="site-footer__inner">
      <p>Brian & Sandra Law | Client-focused representation for families and businesses.</p>
      <p>123 Justice Avenue, Suite 400, Columbus, OH 43215 | (614) 555-0134</p>
      <p>{{ new Date().getFullYear() }} Brian & Sandra Law. All rights reserved.</p>
    </div>
  </footer>
</template>

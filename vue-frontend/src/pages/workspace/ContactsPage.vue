<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ApiClientError, getJson, postJson } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'

type Contact = {
  id: number
  display_name: string
  contact_type: 'Person' | 'Organization'
  primary_email: string | null
  primary_phone: string | null
}

type CreateContactPayload = {
  display_name: string
  contact_type: 'Person' | 'Organization'
  primary_email: string | null
  primary_phone: string | null
}

const auth = useAuthSession()
const contacts = ref<Contact[]>([])
const query = ref('')
const form = ref({
  display_name: '',
  contact_type: 'Person' as 'Person' | 'Organization',
  primary_email: '',
  primary_phone: '',
})
const error = ref<string | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)

async function loadContacts() {
  const token = auth.token.value
  if (!token) return

  isLoading.value = true
  error.value = null
  try {
    contacts.value = await getJson<Contact[]>(`/tenant/contacts?q=${encodeURIComponent(query.value)}`, token)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load contacts.'
  } finally {
    isLoading.value = false
  }
}

async function createContact() {
  const token = auth.token.value
  if (!token) return

  isSaving.value = true
  error.value = null
  try {
    const payload: CreateContactPayload = {
      display_name: form.value.display_name.trim(),
      contact_type: form.value.contact_type,
      primary_email: form.value.primary_email || null,
      primary_phone: form.value.primary_phone || null,
    }

    await postJson<Contact, CreateContactPayload>(
      '/tenant/contacts',
      payload,
      token,
    )
    form.value = {
      display_name: '',
      contact_type: 'Person',
      primary_email: '',
      primary_phone: '',
    }
    await loadContacts()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create contact.'
  } finally {
    isSaving.value = false
  }
}

onMounted(() => {
  void loadContacts()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Contacts</p>
      <h1>Contact directory</h1>
    </header>

    <div class="content-grid">
      <article class="card workspace-form-card">
        <h2>Create contact</h2>
        <form class="workspace-form" @submit.prevent="createContact">
          <label>
            Name
            <input v-model="form.display_name" type="text" required />
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
          <button type="submit" :disabled="isSaving">
            {{ isSaving ? 'Saving...' : 'Create contact' }}
          </button>
        </form>
      </article>

      <article class="card">
        <h2>Directory</h2>
        <label class="workspace-form__inline">
          Search
          <input v-model="query" type="text" @change="loadContacts" />
        </label>
        <p v-if="isLoading" class="muted">Loading contacts...</p>
        <ul class="record-list">
          <li v-for="contact in contacts" :key="contact.id">
            <strong>{{ contact.display_name }}</strong>
            <span>{{ contact.contact_type }}</span>
            <span>{{ contact.primary_email ?? 'No email on file' }}</span>
            <span>{{ contact.primary_phone ?? 'No phone on file' }}</span>
          </li>
        </ul>
      </article>
    </div>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

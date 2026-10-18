<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'

const auth = useAuthSession()
const route = useRoute()
const router = useRouter()

const email = ref('attorney@matterforge.local')
const password = ref('Passw0rd!')
const error = ref<string | null>(null)

async function submit() {
  error.value = null
  try {
    await auth.signIn(email.value.trim(), password.value)
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/workspace'
    await router.push(redirect)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to sign in.'
  }
}
</script>

<template>
  <main class="page">
    <section class="card workspace-login-card">
      <p class="eyebrow">Workspace sign in</p>
      <h1>Access case operations</h1>
      <p class="muted">Use seeded migration credentials to access workspace routes and operational queues.</p>

      <form class="workspace-form" @submit.prevent="submit">
        <label>
          Email
          <input v-model="email" type="email" />
        </label>
        <label>
          Password
          <input v-model="password" type="password" />
        </label>
        <p v-if="error" class="error-message">{{ error }}</p>
        <button type="submit" class="hero-button hero-button--primary" :disabled="auth.isSigningIn.value">
          {{ auth.isSigningIn.value ? 'Signing in...' : 'Sign in' }}
        </button>
      </form>
    </section>
  </main>
</template>

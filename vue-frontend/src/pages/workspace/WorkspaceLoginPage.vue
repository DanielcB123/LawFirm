<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { isPlatformHost } from '../../features/host/surface'

const auth = useAuthSession()
const route = useRoute()
const router = useRouter()

const email = ref('partner@matterforge.local')
const password = ref('Passw0rd!')
const error = ref<string | null>(null)

function safeAuthenticatedRedirect(value: unknown): string {
  if (typeof value !== 'string') {
    return '/workspace'
  }

  const allowedPrefixes = ['/workspace', '/onboarding']
  const isAllowed = allowedPrefixes.some((prefix) => value.startsWith(prefix))
  if (!isAllowed) {
    return '/workspace'
  }

  if (value.startsWith('//')) {
    return '/workspace'
  }

  return value
}

async function submit() {
  error.value = null
  try {
    await auth.signIn(email.value.trim(), password.value)
    const memberships = auth.actor.value?.memberships ?? []
    const isPlatformOwner = auth.actor.value?.platformRole === 'platform_owner'

    if (isPlatformHost() && isPlatformOwner) {
      await router.push('/workspace/platform/firms')
      return
    }

    if (isPlatformHost() && memberships.length > 1) {
      await router.push('/workspace/select-firm')
      return
    }

    if (isPlatformHost() && memberships.length === 1) {
      const membership = memberships[0]
      auth.selectFirm(membership.firmId)
    }

    const redirect = safeAuthenticatedRedirect(route.query.redirect)
    await router.push(redirect)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to sign in.'
  }
}
</script>

<template>
  <section class="card workspace-login-card">
    <p class="eyebrow">Workspace sign in</p>
    <h1>Access case operations</h1>
    <p class="muted">Use seeded migration credentials to access workspace routes and operational queues.</p>
    <p class="muted">
      SaaS platform admin (local seed):
      <strong>partner@matterforge.local</strong>
    </p>

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
</template>

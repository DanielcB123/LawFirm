<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { isPlatformHost, isTenantHost } from '../../features/host/surface'

const auth = useAuthSession()
const route = useRoute()
const router = useRouter()

const email = ref('partner@matterforge.local')
const password = ref('Passw0rd!')
const error = ref<string | null>(null)
const tenantDemoHintEmail = computed(() => {
  if (!isTenantHost()) {
    return null
  }

  const host = typeof window === 'undefined' ? '' : window.location.hostname.toLowerCase()
  const slug = host.endsWith('.firms.localhost') ? host.replace('.firms.localhost', '') : null
  if (!slug || slug === '') {
    return null
  }

  return `attorney.${slug}.firms@localhost`
})

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
    const activeMemberships = memberships.filter((membership) => membership.lifecycleStatus === 'active')
    const isPlatformOwner = auth.actor.value?.platformRole === 'platform_owner'

    if (isPlatformHost() && isPlatformOwner) {
      await router.push('/workspace/platform/firms')
      return
    }

    if (activeMemberships.length === 0) {
      await router.push('/onboarding/status')
      return
    }

    if (isPlatformHost() && activeMemberships.length > 1) {
      await router.push('/workspace/select-firm')
      return
    }

    if (isPlatformHost() && activeMemberships.length === 1) {
      const membership = activeMemberships[0]
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
    <p v-if="isPlatformHost()" class="muted">
      SaaS platform admin (local seed):
      <strong>partner@matterforge.local</strong>
    </p>
    <p v-else-if="tenantDemoHintEmail" class="muted">
      Tenant demo user for this firm:
      <strong>{{ tenantDemoHintEmail }}</strong>
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

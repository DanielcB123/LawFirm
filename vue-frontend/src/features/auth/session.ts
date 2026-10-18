import { computed, ref } from 'vue'
import { getJson, postJson } from '../../lib/apiClient'
import type { ActorIdentity, LoginResponse } from '../../types/auth'

const tokenStorageKey = 'matterforge.session.token'

const token = ref<string | null>(null)
const actor = ref<ActorIdentity | null>(null)
const initialized = ref(false)
const isSigningIn = ref(false)

function initializeFromStorage() {
  if (initialized.value) {
    return
  }

  token.value = localStorage.getItem(tokenStorageKey)
  initialized.value = true
}

function persistToken(nextToken: string | null) {
  token.value = nextToken

  if (nextToken) {
    localStorage.setItem(tokenStorageKey, nextToken)
  } else {
    localStorage.removeItem(tokenStorageKey)
  }
}

async function signIn(email: string, password: string): Promise<void> {
  isSigningIn.value = true
  try {
    const payload = await postJson<LoginResponse, { email: string; password: string }>('/auth/login', {
      email,
      password,
    })

    persistToken(payload.access_token)
    actor.value = payload.user
  } finally {
    isSigningIn.value = false
  }
}

async function loadIdentity(): Promise<void> {
  if (!token.value) {
    actor.value = null
    return
  }

  actor.value = await getJson<ActorIdentity>('/identity/me', token.value)
}

async function ensureSessionLoaded(): Promise<void> {
  initializeFromStorage()

  if (token.value && !actor.value) {
    try {
      await loadIdentity()
    } catch {
      signOut()
    }
  }
}

function signOut() {
  actor.value = null
  persistToken(null)
}

function hasPermission(permission: string): boolean {
  return !!actor.value?.permissions.includes(permission)
}

export function useAuthSession() {
  return {
    token: computed(() => token.value),
    actor: computed(() => actor.value),
    initialized: computed(() => initialized.value),
    isSigningIn: computed(() => isSigningIn.value),
    isAuthenticated: computed(() => !!token.value),
    initializeFromStorage,
    ensureSessionLoaded,
    signIn,
    signOut,
    loadIdentity,
    hasPermission,
  }
}

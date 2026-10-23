import { computed, ref } from 'vue'
import { getJson, postJson } from '../../lib/apiClient'
import type { ActorIdentity, LoginResponse } from '../../types/auth'

const tokenStorageKey = 'matterforge.session.token'
const selectedFirmStorageKey = 'matterforge.session.selectedFirmId'

const token = ref<string | null>(null)
const actor = ref<ActorIdentity | null>(null)
const initialized = ref(false)
const isSigningIn = ref(false)
const selectedFirmId = ref<string | null>(null)

function initializeFromStorage() {
  if (initialized.value) {
    return
  }

  token.value = localStorage.getItem(tokenStorageKey)
  selectedFirmId.value = localStorage.getItem(selectedFirmStorageKey)
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

function persistSelectedFirm(nextFirmId: string | null) {
  selectedFirmId.value = nextFirmId

  if (nextFirmId) {
    localStorage.setItem(selectedFirmStorageKey, nextFirmId)
  } else {
    localStorage.removeItem(selectedFirmStorageKey)
  }
}

function reconcileSelectedFirm(): void {
  const memberships = actor.value?.memberships ?? []
  const allowedFirmIds = new Set(memberships.map((membership) => membership.firmId))

  if (selectedFirmId.value && !allowedFirmIds.has(selectedFirmId.value)) {
    persistSelectedFirm(null)
  }

  if (!selectedFirmId.value && memberships.length === 1) {
    persistSelectedFirm(memberships[0].firmId)
  }
}

function applyLoginResponse(payload: LoginResponse): void {
  persistToken(payload.access_token)
  actor.value = payload.user
  reconcileSelectedFirm()
}

async function signIn(email: string, password: string): Promise<void> {
  isSigningIn.value = true
  try {
    const payload = await postJson<LoginResponse, { email: string; password: string }>('/auth/login', {
      email,
      password,
    })

    applyLoginResponse(payload)
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
  reconcileSelectedFirm()
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
  persistSelectedFirm(null)
}

function hasPermission(permission: string): boolean {
  return !!actor.value?.permissions.includes(permission)
}

function hasPlatformPermission(permission: string): boolean {
  return !!actor.value?.platformPermissions?.includes(permission)
}

function selectFirm(firmId: string | null): void {
  persistSelectedFirm(firmId)
}

export function useAuthSession() {
  return {
    token: computed(() => token.value),
    actor: computed(() => actor.value),
    initialized: computed(() => initialized.value),
    isSigningIn: computed(() => isSigningIn.value),
    isAuthenticated: computed(() => !!token.value),
    selectedFirmId: computed(() => selectedFirmId.value),
    initializeFromStorage,
    ensureSessionLoaded,
    signIn,
    signOut,
    loadIdentity,
    hasPermission,
    hasPlatformPermission,
    selectFirm,
    applyLoginResponse,
  }
}

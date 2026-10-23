<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import {
  addPlatformFirmMembership,
  claimPlatformFirmDomain,
  createPlatformFirm,
  listPlatformFirms,
  provisionPlatformFirm,
  reactivatePlatformFirm,
  reviewPlatformFirm,
  savePlatformFirmBranding,
  savePlatformFirmVerification,
  suspendPlatformFirm,
  verifyPlatformFirmDomain,
  type PlatformFirm,
} from '../../features/platform/api'

const auth = useAuthSession()
const firms = ref<PlatformFirm[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const error = ref<string | null>(null)
const statusFilter = ref('')
const searchFilter = ref('')
const verificationTypes = ['business_existence', 'applicant_authority', 'applicant_email', 'domain_ownership']

const createForm = ref({
  legalName: '',
  displayName: '',
})

const canManage = computed(() => auth.actor.value?.platformRole === 'platform_owner')
const canCreateFirms = computed(() => auth.hasPlatformPermission('platform.firms.create'))
const canRecordVerification = computed(() => auth.hasPlatformPermission('platform.firms.verifications.record'))
const canReview = computed(() => auth.hasPlatformPermission('platform.firms.review.finalize'))
const canProvision = computed(() => auth.hasPlatformPermission('platform.firms.provision'))
const canLifecycle = computed(() => auth.hasPlatformPermission('platform.firms.lifecycle.manage'))
const canManageDomains = computed(() => auth.hasPlatformPermission('platform.firms.domains.manage'))
const canFinalizeDomains = computed(() => auth.hasPlatformPermission('platform.firms.domains.finalize'))
const canManageBranding = computed(() => auth.hasPlatformPermission('platform.firms.branding.manage'))
const canManageMemberships = computed(() => auth.hasPlatformPermission('platform.firms.memberships.manage'))

async function loadFirms() {
  const token = auth.token.value
  if (!token || !canManage.value) {
    firms.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    const payload = await listPlatformFirms(token, statusFilter.value, searchFilter.value)
    firms.value = payload.data
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load firms.'
  } finally {
    isLoading.value = false
  }
}

async function createFirm() {
  const token = auth.token.value
  if (!token) return

  isSaving.value = true
  error.value = null
  try {
    await createPlatformFirm(token, {
      legalName: createForm.value.legalName.trim(),
      displayName: createForm.value.displayName.trim(),
    })
    createForm.value = { legalName: '', displayName: '' }
    await loadFirms()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create firm.'
  } finally {
    isSaving.value = false
  }
}

async function runAction(action: () => Promise<unknown>) {
  error.value = null
  try {
    await action()
    await loadFirms()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Platform action failed.'
  }
}

onMounted(() => {
  void loadFirms()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Platform administration</p>
      <h1>Firm onboarding</h1>
      <p class="muted">
        Manage review, provisioning, domain verification, branding, and first memberships from the control plane.
      </p>
    </header>

    <p v-if="!canManage" class="error-message">This page is available only to platform administrators.</p>

    <template v-else>
      <article class="card workspace-form-card">
        <h2>Create firm</h2>
        <p v-if="!canCreateFirms" class="muted">Your platform role can view firms but cannot create them.</p>
        <form class="workspace-form" @submit.prevent="createFirm">
          <label>
            Legal name
            <input v-model="createForm.legalName" type="text" required />
          </label>
          <label>
            Public display name
            <input v-model="createForm.displayName" type="text" required />
          </label>
          <button type="submit" :disabled="!canCreateFirms || isSaving">
            {{ isSaving ? 'Saving...' : 'Create pending firm' }}
          </button>
        </form>
      </article>

      <article class="card workspace-form-card">
        <h2>Search firms</h2>
        <div class="workspace-form">
          <label>
            Status
            <select v-model="statusFilter" @change="loadFirms">
              <option value="">All</option>
              <option value="pending_review">Pending review</option>
              <option value="approved">Approved</option>
              <option value="active">Active</option>
              <option value="rejected">Rejected</option>
              <option value="suspended">Suspended</option>
            </select>
          </label>
          <label>
            Search
            <input v-model="searchFilter" type="text" @change="loadFirms" />
          </label>
        </div>
      </article>

      <p v-if="isLoading" class="muted">Loading firms...</p>

      <article v-for="firm in firms" :key="firm.id" class="card">
        <h2>{{ firm.displayName }}</h2>
        <p class="muted">
          {{ firm.legalName }} - {{ firm.lifecycleStatus }} / {{ firm.provisioningStatus }}
        </p>
        <p v-if="firm.onboarding.hasApplication" class="muted">
          Revision {{ firm.onboarding.revision }} · Approvals {{ firm.onboarding.approvalCount }} /
          {{ firm.onboarding.requiredApprovals }}
        </p>
        <p v-if="firm.provisioningFailureReason" class="error-message">{{ firm.provisioningFailureReason }}</p>

        <div class="workspace-form__inline">
          <button
            v-if="canRecordVerification"
            v-for="verificationType in verificationTypes"
            :key="`${firm.id}-${verificationType}`"
            type="button"
            @click="
              runAction(async () =>
                savePlatformFirmVerification(auth.token.value!, firm.id, {
                  verificationType,
                  status: 'verified',
                  method: 'manual',
                  notes: 'Manual reviewer verification completed.',
                }),
              )
            "
          >
            Verify {{ verificationType }}
          </button>
        </div>

        <div class="workspace-form__inline">
          <button
            v-if="canReview"
            type="button"
            @click="
              runAction(async () =>
                reviewPlatformFirm(auth.token.value!, firm.id, {
                  decision: 'approve',
                  reason: 'Manual platform review approved.',
                  evidenceReferences: ['manual:review'],
                }),
              )
            "
          >
            Approve
          </button>
          <button
            v-if="canReview"
            type="button"
            @click="
              runAction(async () =>
                reviewPlatformFirm(auth.token.value!, firm.id, {
                  decision: 'reject',
                  reason: 'Manual review rejected.',
                  evidenceReferences: ['manual:review'],
                }),
              )
            "
          >
            Reject
          </button>
          <button
            v-if="canProvision"
            type="button"
            @click="runAction(async () => provisionPlatformFirm(auth.token.value!, firm.id))"
          >
            Provision
          </button>
          <button
            v-if="canLifecycle"
            type="button"
            @click="runAction(async () => suspendPlatformFirm(auth.token.value!, firm.id, 'Manual platform suspension.'))"
          >
            Suspend
          </button>
          <button
            v-if="canLifecycle"
            type="button"
            @click="runAction(async () => reactivatePlatformFirm(auth.token.value!, firm.id, 'Manual reactivation.'))"
          >
            Reactivate
          </button>
        </div>

        <div class="workspace-form__inline">
          <button
            v-if="canManageDomains"
            type="button"
            @click="
              runAction(async () =>
                claimPlatformFirmDomain(auth.token.value!, firm.id, {
                  hostname: `${firm.slug}.example.test`,
                  primary: true,
                }),
              )
            "
          >
            Claim domain
          </button>
          <button
            v-if="canFinalizeDomains && firm.domains[0]"
            type="button"
            @click="runAction(async () => verifyPlatformFirmDomain(auth.token.value!, firm.id, firm.domains[0].id))"
          >
            Verify first domain
          </button>
          <button
            v-if="canManageBranding"
            type="button"
            @click="
              runAction(async () =>
                savePlatformFirmBranding(auth.token.value!, firm.id, {
                  logoPath: null,
                  faviconPath: null,
                  colors: { primary: '#2F1F1A', secondary: '#4D3F36', accent: '#5D1F2F' },
                  typography: 'system-sans',
                  contactEmail: null,
                  contactPhone: null,
                  contactAddress: null,
                  publicCopy: firm.displayName,
                  publicSiteKey: firm.publicSiteKey,
                  enabledModules: ['public_site', 'consultations'],
                }),
              )
            "
          >
            Save branding
          </button>
          <button
            v-if="canManageMemberships"
            type="button"
            @click="runAction(async () => addPlatformFirmMembership(auth.token.value!, firm.id, { userId: 1, role: 'Partner' }))"
          >
            Add seeded admin membership
          </button>
        </div>
      </article>
    </template>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

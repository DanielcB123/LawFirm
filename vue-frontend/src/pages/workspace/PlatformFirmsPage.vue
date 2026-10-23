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
  reopenPlatformFirm,
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
const activeVerificationKey = ref<string | null>(null)
const verificationStatus = ref<'in_review' | 'verified' | 'rejected' | 'unavailable'>('in_review')
const verificationMethod = ref<'manual' | 'provider'>('manual')
const verificationNotes = ref('')
const verificationEvidence = ref<Record<string, string>>({})
const verificationStatusCopy: Record<string, string> = {
  pending: 'Pending',
  in_review: 'In review',
  verified: 'Verified',
  rejected: 'Rejected',
  unavailable: 'Unavailable',
  bypassed: 'Bypassed',
}

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

function workflowKey(firmId: string, verificationType: string): string {
  return `${firmId}:${verificationType}`
}

function openVerificationWorkflow(firm: PlatformFirm, verificationType: string): void {
  const verification = firm.onboarding.verificationStatuses.find((item) => item.type === verificationType)
  activeVerificationKey.value = workflowKey(firm.id, verificationType)
  verificationStatus.value = verification?.status === 'verified' ? 'verified' : 'in_review'
  verificationMethod.value = verification?.method === 'provider' ? 'provider' : 'manual'
  verificationNotes.value = verification?.notes ?? ''
  verificationEvidence.value = { ...(verification?.evidence ?? {}) }
}

function closeVerificationWorkflow(): void {
  activeVerificationKey.value = null
  verificationNotes.value = ''
  verificationEvidence.value = {}
}

async function saveVerification(firm: PlatformFirm, verificationType: string): Promise<void> {
  const token = auth.token.value
  if (!token) {
    return
  }

  await runAction(async () => {
    await savePlatformFirmVerification(token, firm.id, {
      verificationType,
      status: verificationStatus.value,
      method: verificationMethod.value,
      notes: verificationNotes.value.trim() || null,
      evidence: verificationEvidence.value,
    })
  })
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
          Revision {{ firm.onboarding.revision }}
          <template v-if="firm.onboarding.requiredApprovals > 0">
            · Approvals {{ firm.onboarding.approvalCount }} / {{ firm.onboarding.requiredApprovals }}
          </template>
        </p>
        <p v-if="firm.onboarding.devSimplifiedFlow" class="muted">
          Simplified dev flow is enabled: complete verifications, then provision directly.
        </p>
        <p v-if="firm.lifecycleStatus === 'rejected' && !firm.onboarding.devSimplifiedFlow" class="error-message">
          This application is currently rejected. Reopen it before collecting approvals again.
        </p>
        <p v-if="firm.lifecycleStatus === 'rejected' && firm.onboarding.devSimplifiedFlow" class="muted">
          Rejected state will be auto-promoted to approved during provisioning when verifications are complete.
        </p>
        <p v-if="firm.provisioningFailureReason" class="error-message">{{ firm.provisioningFailureReason }}</p>

        <article v-if="firm.onboarding.hasApplication" class="card workspace-form-card">
          <h3>Verification workflows</h3>
          <p class="muted">Click a verification row to open the multi-step workflow.</p>
          <ul class="firm-selector">
            <li
              v-for="verification in firm.onboarding.verificationStatuses"
              :id="`verify-${firm.id}-${verification.type}`"
              :key="`${firm.id}-${verification.type}`"
              class="firm-selector__item"
              role="button"
              tabindex="0"
              @click="openVerificationWorkflow(firm, verification.type)"
              @keydown.enter.prevent="openVerificationWorkflow(firm, verification.type)"
              @keydown.space.prevent="openVerificationWorkflow(firm, verification.type)"
            >
              <span class="firm-selector__title">{{ verification.label }}</span>
              <span class="firm-selector__meta">
                {{ verificationStatusCopy[verification.status] ?? verification.status }} · {{ verification.method }}
              </span>
              <span class="firm-selector__meta">Open workflow</span>
            </li>
          </ul>

          <div
            v-for="verification in firm.onboarding.verificationStatuses"
            v-show="activeVerificationKey === workflowKey(firm.id, verification.type)"
            :key="`${firm.id}-${verification.type}-workflow`"
            class="workspace-form"
          >
            <h4>{{ verification.label }}</h4>
            <p class="muted">{{ verification.workflow.description }}</p>
            <ol class="muted">
              <li v-for="step in verification.workflow.steps" :key="step">{{ step }}</li>
            </ol>
            <label>
              Status
              <select v-model="verificationStatus">
                <option value="in_review">In review</option>
                <option value="verified">Verified</option>
                <option value="rejected">Rejected</option>
                <option value="unavailable">Unavailable</option>
              </select>
            </label>
            <label>
              Method
              <select v-model="verificationMethod">
                <option value="manual">Manual</option>
                <option value="provider">Provider</option>
              </select>
            </label>
            <label v-for="field in verification.workflow.requiredFields" :key="field.key">
              {{ field.label }}
              <input v-model="verificationEvidence[field.key]" type="text" />
              <span v-if="field.helpText" class="muted">{{ field.helpText }}</span>
            </label>
            <label>
              Reviewer notes
              <textarea v-model="verificationNotes" rows="2" />
            </label>
            <p v-if="!canRecordVerification" class="muted">
              Your platform role can view workflow steps but cannot save verification decisions.
            </p>
            <div class="workspace-form__inline">
              <button type="button" :disabled="!canRecordVerification" @click="saveVerification(firm, verification.type)">
                Save verification
              </button>
              <button type="button" @click="closeVerificationWorkflow">Close workflow</button>
            </div>
          </div>
        </article>

        <article class="card workspace-form-card">
          <h3>Platform actions</h3>
          <p class="muted">These are control-plane actions for lifecycle, domain, branding, and memberships.</p>

          <h4>Lifecycle and review</h4>
          <div class="workspace-form__inline">
            <button
              v-if="canReview && !firm.onboarding.devSimplifiedFlow && firm.lifecycleStatus === 'rejected'"
              type="button"
              @click="
                runAction(async () =>
                  reopenPlatformFirm(auth.token.value!, firm.id, 'Reopened after verification evidence review.'),
                )
              "
            >
              Reopen application
            </button>
            <button
              v-if="canReview && !firm.onboarding.devSimplifiedFlow && firm.lifecycleStatus !== 'rejected'"
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
              v-if="canReview && !firm.onboarding.devSimplifiedFlow && firm.lifecycleStatus !== 'rejected'"
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

          <h4>Domain, branding, and membership</h4>
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
      </article>
    </template>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>

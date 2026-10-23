<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { getOnboardingStatus, runFastTrackOnboarding, type OnboardingStatusResponse } from '../../features/onboarding/api'

const auth = useAuthSession()
const status = ref<OnboardingStatusResponse | null>(null)
const error = ref<string | null>(null)
const isLoading = ref(false)
const isFastTracking = ref(false)

const application = computed(() => status.value?.application)
const developmentNotice = computed(() => status.value?.developmentNotice ?? null)
const canFastTrack = computed(
  () =>
    status.value?.fastTrackEnabled === true &&
    application.value &&
    application.value.stage !== 'active' &&
    application.value.stage !== 'provisioning',
)

const stageCopy: Record<string, string> = {
  pending_review: 'Pending review',
  awaiting_second_approval: 'Awaiting second approval',
  rejected: 'Rejected',
  provisioning: 'Provisioning',
  provisioning_failed: 'Provisioning failed',
  active: 'Active',
}

async function loadStatus(): Promise<void> {
  const token = auth.token.value
  if (!token) {
    return
  }

  isLoading.value = true
  error.value = null
  try {
    status.value = await getOnboardingStatus(token)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load onboarding status.'
  } finally {
    isLoading.value = false
  }
}

async function fastTrack(): Promise<void> {
  const token = auth.token.value
  if (!token) {
    return
  }

  isFastTracking.value = true
  error.value = null
  try {
    status.value = await runFastTrackOnboarding(token)
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to run local fast-track.'
  } finally {
    isFastTracking.value = false
  }
}

onMounted(() => {
  void loadStatus()
})
</script>

<template>
  <section class="card workspace-login-card">
    <p class="eyebrow">Firm onboarding</p>
    <h1>Application status</h1>
    <p class="muted">Track verification, reviewer approvals, provisioning, and workspace activation.</p>

    <p v-if="isLoading" class="muted">Loading status...</p>
    <p v-if="error" class="error-message">{{ error }}</p>

    <template v-if="status && !status.hasApplication">
      <p class="muted">No application found for your account.</p>
      <RouterLink class="hero-button hero-button--primary" to="/register-firm">Register your firm</RouterLink>
    </template>

    <template v-else-if="application">
      <div class="workspace-form">
        <label>
          Firm
          <input :value="application.firmName" type="text" disabled />
        </label>
        <label>
          Current stage
          <input :value="stageCopy[application.stage] ?? application.stage" type="text" disabled />
        </label>
        <label>
          Approvals
          <input :value="`${application.approvalCount}/${application.requiredApprovals}`" type="text" disabled />
        </label>
      </div>

      <p v-if="application.provisioningFailureReason" class="error-message">{{ application.provisioningFailureReason }}</p>
      <p v-if="developmentNotice" class="muted">{{ developmentNotice }}</p>

      <div class="workspace-form__inline">
        <button v-if="canFastTrack" type="button" @click="fastTrack" :disabled="isFastTracking">
          {{ isFastTracking ? 'Provisioning...' : 'Run local fast-track' }}
        </button>
        <RouterLink
          v-if="application.stage === 'active' && application.ownerMembershipReady"
          class="hero-button hero-button--primary"
          to="/workspace"
        >
          Enter firm dashboard
        </RouterLink>
      </div>

      <article class="card workspace-form-card">
        <h2>Verification checks</h2>
        <ul class="firm-selector">
          <li v-for="verification in application.verifications" :key="verification.type" class="firm-selector__item">
            <span class="firm-selector__title">{{ verification.type }}</span>
            <span class="firm-selector__meta">{{ verification.status }} · {{ verification.method }}</span>
          </li>
        </ul>
      </article>
    </template>
  </section>
</template>

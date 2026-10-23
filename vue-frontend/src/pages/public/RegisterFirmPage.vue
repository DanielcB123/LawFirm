<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import {
  checkFirmNameAvailability,
  registerFirm,
  submitFirmApplication,
  type FirmNameAvailabilityResponse,
} from '../../features/onboarding/api'

const router = useRouter()
const auth = useAuthSession()

const form = ref({
  firmName: '',
  name: '',
  email: '',
  password: '',
  passwordConfirmation: '',
  businessPhone: '',
})

const error = ref<string | null>(null)
const isSaving = ref(false)
const isCheckingName = ref(false)
const availability = ref<FirmNameAvailabilityResponse | null>(null)
const availabilityError = ref<string | null>(null)
let availabilityTimer: ReturnType<typeof setTimeout> | null = null

const isAuthenticated = computed(() => auth.isAuthenticated.value)
const trimmedFirmName = computed(() => form.value.firmName.trim())
const canSubmit = computed(() => {
  if (isCheckingName.value) {
    return false
  }

  if (availability.value && !availability.value.available) {
    return false
  }

  return true
})

async function checkName(): Promise<void> {
  const firmName = trimmedFirmName.value
  if (firmName.length < 3) {
    availability.value = null
    availabilityError.value = null
    return
  }

  isCheckingName.value = true
  availabilityError.value = null
  try {
    availability.value = await checkFirmNameAvailability(firmName)
  } catch (err) {
    availability.value = null
    availabilityError.value = err instanceof ApiClientError ? err.message : 'Unable to check firm name availability.'
  } finally {
    isCheckingName.value = false
  }
}

function applySuggestedSlug(slug: string): void {
  form.value.firmName = slug
}

async function submit(): Promise<void> {
  error.value = null
  isSaving.value = true

  try {
    if (isAuthenticated.value && auth.token.value) {
      await submitFirmApplication(auth.token.value, {
        firmName: form.value.firmName.trim(),
        applicantName: form.value.name.trim() || auth.actor.value?.name,
        applicantEmail: form.value.email.trim() || auth.actor.value?.email,
        businessPhone: form.value.businessPhone.trim() || null,
      })
    } else {
      const payload = await registerFirm({
        firmName: form.value.firmName.trim(),
        name: form.value.name.trim(),
        email: form.value.email.trim().toLowerCase(),
        password: form.value.password,
        password_confirmation: form.value.passwordConfirmation,
        businessPhone: form.value.businessPhone.trim() || null,
      })

      auth.applyLoginResponse(payload)
    }

    await router.push({ name: 'onboarding-status' })
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to submit your firm registration.'
  } finally {
    isSaving.value = false
  }
}

watch(
  () => form.value.firmName,
  () => {
    if (availabilityTimer) {
      clearTimeout(availabilityTimer)
    }

    availabilityTimer = setTimeout(() => {
      void checkName()
    }, 300)
  },
)
</script>

<template>
  <section class="card workspace-login-card">
    <p class="eyebrow">Firm onboarding</p>
    <h1>Register your firm</h1>
    <p class="muted">
      Submit your firm application for verification, two-person review, and tenant workspace provisioning.
    </p>

    <form class="workspace-form" @submit.prevent="submit">
      <label>
        Firm name
        <input v-model="form.firmName" type="text" required />
      </label>
      <p v-if="isCheckingName" class="muted">Checking firm URL availability...</p>
      <p v-else-if="availability && availability.available" class="muted">
        Available: <strong>{{ availability.slug }}</strong>.firms.localhost
      </p>
      <div v-else-if="availability && !availability.available">
        <p class="error-message">{{ availability.message }}</p>
        <p class="muted">Suggested available names:</p>
        <div class="workspace-form__inline">
          <button
            v-for="suggestion in availability.suggestions"
            :key="suggestion"
            type="button"
            @click="applySuggestedSlug(suggestion)"
          >
            {{ suggestion }}
          </button>
        </div>
      </div>
      <p v-if="availabilityError" class="error-message">{{ availabilityError }}</p>

      <label>
        Applicant name
        <input
          v-model="form.name"
          type="text"
          :required="!isAuthenticated"
          :placeholder="isAuthenticated ? auth.actor.value?.name : ''"
        />
      </label>

      <label>
        Applicant email
        <input
          v-model="form.email"
          type="email"
          :required="!isAuthenticated"
          :placeholder="isAuthenticated ? auth.actor.value?.email : ''"
        />
      </label>

      <label>
        Business phone (optional)
        <input v-model="form.businessPhone" type="text" />
      </label>

      <template v-if="!isAuthenticated">
        <label>
          Password
          <input v-model="form.password" type="password" minlength="8" required />
        </label>
        <label>
          Confirm password
          <input v-model="form.passwordConfirmation" type="password" minlength="8" required />
        </label>
      </template>

      <p v-if="error" class="error-message">{{ error }}</p>
      <button type="submit" class="hero-button hero-button--primary" :disabled="isSaving || !canSubmit">
        {{ isSaving ? 'Submitting...' : 'Submit firm application' }}
      </button>
    </form>
  </section>
</template>

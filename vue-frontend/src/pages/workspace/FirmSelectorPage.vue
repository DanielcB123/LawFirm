<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthSession } from '../../features/auth/session'

const auth = useAuthSession()
const router = useRouter()

const memberships = computed(() =>
  (auth.actor.value?.memberships ?? []).filter((membership) => membership.lifecycleStatus === 'active'),
)

async function openFirmWorkspace(firmId: string) {
  auth.selectFirm(firmId)
  await router.push('/workspace')
}
</script>

<template>
  <section class="card workspace-login-card">
    <p class="eyebrow">Select firm workspace</p>
    <h1>Choose your firm</h1>
    <p class="muted">Your account belongs to multiple firms. Select one workspace to continue.</p>

    <div class="firm-selector">
      <button
        v-for="membership in memberships"
        :key="membership.firmId"
        type="button"
        class="firm-selector__item"
        @click="openFirmWorkspace(membership.firmId)"
      >
        <span class="firm-selector__title">{{ membership.displayName }}</span>
        <span class="firm-selector__meta">{{ membership.role }} · {{ membership.lifecycleStatus }}</span>
        <span class="firm-selector__host">{{ membership.preferredHost || membership.fallbackHost || 'Current host' }}</span>
      </button>
    </div>
  </section>
</template>


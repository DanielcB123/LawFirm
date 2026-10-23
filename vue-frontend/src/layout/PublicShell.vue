<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import SiteNav from '../components/layout/SiteNav.vue'
import SiteFooter from '../components/layout/SiteFooter.vue'
import ConsultationModalHost from '../components/consultation/ConsultationModalHost.vue'
import { useScrollReveal } from '../composables/useScrollReveal'
import {
  applyPublicBranding,
  loadPublicBranding,
  registeredFirmExtensionCount,
  usePublicBrandingState,
} from '../features/branding/api'
import { platformBranding } from '../features/branding/types'
import { builtInFirmExtensions } from '../firms/extensions'

const route = useRoute()
useScrollReveal(() => route.fullPath)
const branding = usePublicBrandingState()

onMounted(async () => {
  try {
    applyPublicBranding(await loadPublicBranding())
  } catch {
    applyPublicBranding(platformBranding)
  }
})
</script>

<template>
  <div
    class="public-shell"
    :class="builtInFirmExtensions[branding.publicSiteKey].shellClass"
    :data-registered-firm-extensions="registeredFirmExtensionCount()"
  >
    <SiteNav />
    <main class="page">
      <RouterView />
    </main>
    <SiteFooter />
    <ConsultationModalHost />
  </div>
</template>

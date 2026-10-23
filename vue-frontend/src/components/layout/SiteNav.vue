<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { loadPublicBranding } from '../../features/branding/api'
import { platformBranding } from '../../features/branding/types'
import { isPlatformHost } from '../../features/host/surface'

const displayName = ref(platformBranding.displayName)
const brandingSource = ref<'platform' | 'firm'>('platform')
const platformHost = isPlatformHost()
const isPlatformSurface = computed(() => platformHost && brandingSource.value === 'platform')

onMounted(async () => {
  try {
    const branding = await loadPublicBranding()
    displayName.value = branding.displayName
    brandingSource.value = branding.source
  } catch {
    displayName.value = platformBranding.displayName
    brandingSource.value = 'platform'
  }
})
</script>

<template>
  <header class="site-header">
    <div class="site-header__inner">
      <RouterLink class="site-brand-link" to="/">
        <p class="site-brand">{{ displayName }}</p>
      </RouterLink>
      <nav class="site-nav" aria-label="Primary">
        <RouterLink class="site-nav__link" to="/" active-class="site-nav__link--active">Home</RouterLink>
        <RouterLink
          v-if="isPlatformSurface"
          class="site-nav__link"
          to="/for-law-firms"
          active-class="site-nav__link--active"
        >
          For Law Firms
        </RouterLink>
        <RouterLink
          v-if="isPlatformSurface"
          class="site-nav__link"
          to="/register-firm"
          active-class="site-nav__link--active"
        >
          Register your firm
        </RouterLink>
        <RouterLink class="site-nav__link" to="/about" active-class="site-nav__link--active">About</RouterLink>
        <RouterLink class="site-nav__link" to="/practice-areas" active-class="site-nav__link--active">
          Practice Areas
        </RouterLink>
        <RouterLink class="site-nav__link" to="/attorneys" active-class="site-nav__link--active">Attorneys</RouterLink>
        <RouterLink class="site-nav__link" to="/contact" active-class="site-nav__link--active">Contact</RouterLink>
        <RouterLink class="site-nav__link" to="/workspace/login" active-class="site-nav__link--active">
          {{ isPlatformSurface ? 'Firm sign in' : 'Sign in' }}
        </RouterLink>
      </nav>
    </div>
  </header>
</template>

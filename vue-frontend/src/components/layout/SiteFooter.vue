<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { loadPublicBranding } from '../../features/branding/api'
import { platformBranding, type PublicBranding } from '../../features/branding/types'

const branding = ref<PublicBranding>(platformBranding)
const year = new Date().getFullYear()
const contactLine = computed(() =>
  [branding.value.contactAddress, branding.value.contactPhone].filter((part) => part && part.trim() !== '').join(' | '),
)

onMounted(async () => {
  try {
    branding.value = await loadPublicBranding()
  } catch {
    branding.value = platformBranding
  }
})
</script>

<template>
  <footer class="site-footer">
    <div class="site-footer__inner">
      <p>{{ branding.displayName }} | {{ branding.publicCopy }}</p>
      <p>{{ contactLine }}</p>
      <p>{{ year }} {{ branding.displayName }}. All rights reserved.</p>
    </div>
  </footer>
</template>

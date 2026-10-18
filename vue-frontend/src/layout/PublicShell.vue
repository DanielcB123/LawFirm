<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router'
import SiteNav from '../components/layout/SiteNav.vue'
import SiteFooter from '../components/layout/SiteFooter.vue'
import ConsultationModalHost from '../components/consultation/ConsultationModalHost.vue'
import { useScrollReveal } from '../composables/useScrollReveal'

const route = useRoute()
useScrollReveal(() => route.fullPath)
</script>

<template>
  <div class="public-shell">
    <SiteNav />
    <main class="page">
      <RouterView />
    </main>
    <SiteFooter />
    <ConsultationModalHost />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useConsultationModal } from '../../features/consultations/modal'
import { isPlatformHost } from '../../features/host/surface'
import { usePublicBrandingState } from '../../features/branding/api'

const modal = useConsultationModal()
const branding = usePublicBrandingState()
const isPlatformSurface = computed(() => isPlatformHost() && branding.value.source === 'platform')

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section class="card marketing-hero" data-reveal="zoom">
      <p class="eyebrow">{{ isPlatformSurface ? 'Platform onboarding' : 'Contact us' }}</p>
      <h1>
        {{
          isPlatformSurface
            ? 'Tell us about your law firm and onboarding goals.'
            : 'Tell us what is happening. We will help you plan the next move.'
        }}
      </h1>
      <p class="lead">
        {{
          isPlatformSurface
            ? 'Use this path for law-firm onboarding requests, tenant setup planning, and domain rollout support.'
            : 'Request a consultation to discuss your matter. We respond to all inquiries within one business day.'
        }}
      </p>
      <RouterLink v-if="isPlatformSurface" class="hero-button hero-button--primary" to="/for-law-firms">
        Start firm onboarding
      </RouterLink>
      <button v-else type="button" class="hero-button hero-button--primary" @click="openConsultation">Request appointment</button>
    </section>

    <section class="content-grid marketing-grid" data-reveal="left">
      <article class="card marketing-card">
        <h2>{{ isPlatformSurface ? 'Platform support' : 'Office' }}</h2>
        <p>{{ isPlatformSurface ? 'MatterForge onboarding team' : '123 Justice Avenue, Suite 400' }}</p>
        <p>{{ isPlatformSurface ? 'Domain verification and tenant setup guidance' : 'Columbus, OH 43215' }}</p>
      </article>
      <article class="card marketing-card">
        <h2>{{ isPlatformSurface ? 'Contact channels' : 'Phone & Email' }}</h2>
        <p>{{ isPlatformSurface ? '(614) 555-0101' : '(614) 555-0134' }}</p>
        <p>{{ isPlatformSurface ? 'onboarding@matterforge.local' : 'intake@parkerreedlaw.com' }}</p>
      </article>
      <article class="card marketing-card">
        <h2>Hours</h2>
        <p>Monday-Friday: 8:30 AM-6:00 PM</p>
        <p>Saturday: By appointment</p>
        <p>Sunday: Closed</p>
      </article>
      <article class="card marketing-card">
        <h2>What to prepare</h2>
        <ul>
          <li>Names of all people or businesses involved</li>
          <li>Contracts, notices, or letters related to the issue</li>
          <li>Important dates and deadlines</li>
          <li>Your goals and preferred legal outcome</li>
        </ul>
      </article>
    </section>

  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { teamMemberBySlug } from '../../data/teamMembers'
import { useConsultationModal } from '../../features/consultations/modal'

const route = useRoute()

const member = computed(() => {
  const slug = String(route.params.slug ?? '')
  return teamMemberBySlug[slug]
})

const roleStrengths = {
  Partner: ['Litigation leadership', 'Negotiation strategy', 'Courtroom readiness'],
  Attorney: ['Client counseling', 'Risk analysis', 'Dispute resolution'],
  Paralegal: ['Case operations', 'Filing discipline', 'Deadline control'],
  LegalAssistant: ['Client coordination', 'Intake workflow', 'Scheduling support'],
  Accountant: ['Billing integrity', 'Payment workflow', 'Financial coordination'],
  AdminIT: ['Access controls', 'Systems reliability', 'Security operations'],
} as const

const profileHighlights = computed(() => {
  if (!member.value) {
    return []
  }
  return roleStrengths[member.value.roleGroup]
})

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section v-if="member" class="card attorney-profile attorney-profile--animated" data-reveal="zoom">
      <img :src="member.image" :alt="member.name" class="attorney-profile__image" />
      <div class="attorney-profile__content">
        <p class="eyebrow">Attorney profile</p>
        <h1>{{ member.name }}</h1>
        <p class="attorney-profile__role">{{ member.role }}</p>
        <p class="lead">{{ member.shortBio }}</p>

        <div class="attorney-profile__chips" aria-label="Profile highlights">
          <span v-for="chip in profileHighlights" :key="chip" class="attorney-profile__chip">{{ chip }}</span>
        </div>

        <section class="attorney-profile__panel">
          <h2>What clients can expect</h2>
          <p>
            Clear updates, practical options, and a strategy aligned to outcomes, timeline, and risk tolerance from
            your first consultation onward.
          </p>
        </section>

        <h2>Background</h2>
        <ul>
          <li v-for="point in member.background" :key="point">{{ point }}</li>
        </ul>
        <h2 v-if="member.education?.length">Education</h2>
        <ul v-if="member.education?.length">
          <li v-for="item in member.education" :key="item">{{ item }}</li>
        </ul>
        <h2 v-if="member.admissions?.length">Admissions</h2>
        <ul v-if="member.admissions?.length">
          <li v-for="item in member.admissions" :key="item">{{ item }}</li>
        </ul>
        <div class="attorney-profile__actions">
          <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Contact the firm</button>
          <RouterLink class="hero-button" to="/attorneys">Back to attorneys</RouterLink>
        </div>
      </div>
    </section>

    <section v-else class="card marketing-card">
      <h1>Profile not found</h1>
      <p>The attorney profile you requested could not be located.</p>
      <RouterLink class="hero-button" to="/attorneys">Return to attorneys</RouterLink>
    </section>
  </section>
</template>

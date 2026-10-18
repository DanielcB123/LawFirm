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

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section v-if="member" class="card attorney-profile" data-reveal="zoom">
      <img :src="member.image" :alt="member.name" class="attorney-profile__image" />
      <div class="attorney-profile__content">
        <p class="eyebrow">Attorney profile</p>
        <h1>{{ member.name }}</h1>
        <p class="attorney-profile__role">{{ member.role }}</p>
        <p class="lead">{{ member.shortBio }}</p>
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
        <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Contact the firm</button>
        <RouterLink class="hero-button" to="/attorneys">Back to attorneys</RouterLink>
      </div>
    </section>

    <section v-else class="card marketing-card">
      <h1>Profile not found</h1>
      <p>The attorney profile you requested could not be located.</p>
      <RouterLink class="hero-button" to="/attorneys">Return to attorneys</RouterLink>
    </section>
  </section>
</template>

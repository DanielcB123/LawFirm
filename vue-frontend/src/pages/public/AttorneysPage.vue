<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { teamMembers } from '../../data/teamMembers'
import { useConsultationModal } from '../../features/consultations/modal'

const attorneys = computed(() =>
  teamMembers.filter((member) => ['Partner', 'Attorney'].includes(member.roleGroup)),
)

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section class="card marketing-hero" data-reveal="zoom">
      <p class="eyebrow">Our attorneys</p>
      <h1>Meet the legal team representing your next major decision.</h1>
      <p class="lead">Explore attorney backgrounds, practice focus, and the staff who keep matters moving.</p>
    </section>

    <section class="content-grid marketing-grid" data-reveal>
      <RouterLink
        v-for="attorney in attorneys"
        :key="attorney.slug"
        :to="`/attorneys/${attorney.slug}`"
        class="card marketing-card home-panel--lift attorney-card attorney-card--link"
      >
        <img :src="attorney.image" :alt="attorney.name" loading="lazy" />
        <h2>{{ attorney.name }}</h2>
        <p class="eyebrow attorney-card__role">{{ attorney.role }}</p>
        <p>{{ attorney.shortBio }}</p>
        <span class="attorney-card__action">View full profile</span>
      </RouterLink>
    </section>

    <section class="card marketing-card" data-reveal="left">
      <h2>Operations team supporting every matter</h2>
      <p class="muted">Paralegals, legal assistants, accounting, and admin/IT partners keep each case workflow reliable.</p>
      <ul class="team-list">
        <li v-for="member in teamMembers.filter((x) => !['Partner', 'Attorney'].includes(x.roleGroup))" :key="member.slug">
          <strong>{{ member.name }}</strong> - {{ member.role }}
        </li>
      </ul>
      <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Schedule consultation</button>
    </section>
  </section>
</template>

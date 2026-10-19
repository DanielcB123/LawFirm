<script setup lang="ts">
import { useConsultationModal } from '../../features/consultations/modal'

const practiceAreas = [
  {
    title: 'Business Litigation',
    detail: 'Contract disputes, partnership breakdowns, and risk-aware courtroom strategy.',
  },
  {
    title: 'Civil Litigation',
    detail: 'End-to-end dispute handling from case intake through trial and post-judgment actions.',
  },
  {
    title: 'Estate Planning',
    detail: 'Wills, trusts, powers of attorney, and probate-ready planning frameworks.',
  },
  {
    title: 'Compliance Advisory',
    detail: 'Proactive counsel to reduce legal exposure in ongoing operational decisions.',
  },
]

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section class="card marketing-hero" data-reveal="zoom">
      <p class="eyebrow">Practice areas</p>
      <h1>Focused legal coverage built around your real-world decisions.</h1>
      <p class="lead">
        Our team aligns legal strategy to business and family priorities while preserving trial-readiness when a dispute
        escalates.
      </p>
    </section>

    <section class="content-grid marketing-grid" data-reveal>
      <article class="card marketing-card home-panel--lift" v-for="area in practiceAreas" :key="area.title">
        <h2>{{ area.title }}</h2>
        <p>{{ area.detail }}</p>
      </article>
    </section>

    <section class="card marketing-cta" data-reveal="zoom">
      <p class="marketing-cta__kicker">Case planning</p>
      <h2>Request consultation to align legal strategy with your timeline.</h2>
      <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Request consultation</button>
    </section>
  </section>
</template>

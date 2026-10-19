<script setup lang="ts">
import { useConsultationModal } from '../../features/consultations/modal'

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="marketing-page">
    <section class="card marketing-hero" data-reveal="zoom">
      <p class="eyebrow">About the firm</p>
      <h1>Trial-ready representation with practical, client-first strategy.</h1>
      <p class="lead">
        We serve individuals, families, and businesses with legal counsel grounded in clarity, responsiveness, and
        decisive execution.
      </p>
    </section>

    <section class="content-grid marketing-grid" data-reveal>
      <article class="card marketing-card">
        <h2>How we work</h2>
        <ul>
          <li>Early case triage focused on immediate risk and legal leverage.</li>
          <li>Clear communication and milestone-based strategy updates.</li>
          <li>Prepared negotiation posture with trial-readiness from day one.</li>
        </ul>
      </article>
      <article class="card marketing-card">
        <h2>Who we support</h2>
        <p>
          Our practice supports construction law, civil litigation, and planning matters where clients need practical
          execution, not generic legal theory.
        </p>
        <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Contact our office</button>
      </article>
    </section>
  </section>
</template>

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
      <p class="eyebrow">Contact us</p>
      <h1>Tell us what is happening. We will help you plan the next move.</h1>
      <p class="lead">Request a consultation to discuss your matter. We respond to all inquiries within one business day.</p>
      <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Request appointment</button>
    </section>

    <section class="content-grid marketing-grid" data-reveal="left">
      <article class="card marketing-card">
        <h2>Office</h2>
        <p>123 Justice Avenue, Suite 400</p>
        <p>Columbus, OH 43215</p>
      </article>
      <article class="card marketing-card">
        <h2>Phone & Email</h2>
        <p>(614) 555-0134</p>
        <p>intake@parkerreedlaw.com</p>
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

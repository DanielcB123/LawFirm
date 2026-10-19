<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { teamMembers } from '../../data/teamMembers'
import { useConsultationModal } from '../../features/consultations/modal'

const quickStats = [
  { label: 'Avg. response window', value: '< 1 business day' },
  { label: 'Practice model', value: 'Client-first, trial-ready' },
  { label: 'Consultations', value: 'Same-week availability' },
]

const valueProps = [
  'Responsive communication and transparent billing.',
  'Focused experience in high-stakes civil matters.',
  'Personalized strategy tailored to your goals.',
]

const practiceAreaHighlights = [
  {
    title: 'Business & Contract Disputes',
    body: 'Strategic negotiation and litigation support to protect your company, partnerships, and future growth.',
  },
  {
    title: 'Risk & Compliance Advisory',
    body: 'Proactive legal guidance to reduce exposure, improve policy discipline, and prevent costly disputes.',
  },
  {
    title: 'Civil Litigation',
    body: 'End-to-end courtroom representation from early case strategy through trial and post-judgment enforcement.',
  },
  {
    title: 'Estate & Legacy Planning',
    body: 'Wills, trusts, and planning frameworks that bring clarity and confidence to major life transitions.',
  },
]

const partnerProfiles = teamMembers.filter((member) => member.roleGroup === 'Partner').slice(0, 2)

const processSteps = [
  'Initial consultation to understand your goals, timeline, and constraints.',
  'Case roadmap with clear options, likely outcomes, and next actions.',
  'Execution phase with active communication and transparent milestones.',
  'Resolution and long-term planning to protect what comes next.',
]

const testimonials = [
  {
    quote:
      'They gave us a strategy we could actually follow. Every next step was clear, and we always knew where we stood.',
    source: 'Client - Small business owner',
  },
  {
    quote:
      'Professional, responsive, and honest about options. The process felt manageable because their team stayed ahead of every detail.',
    source: 'Client - Civil litigation matter',
  },
]

const modal = useConsultationModal()

function openConsultation(event: MouseEvent) {
  modal.openModal(event.currentTarget)
}
</script>

<template>
  <section class="home-page">
    <section class="home-hero" data-reveal="zoom">
      <div class="home-hero__image" aria-hidden="true"></div>
      <div class="home-hero__glow" aria-hidden="true"></div>
      <div class="home-hero__grid" aria-hidden="true"></div>
      <p class="eyebrow home-hero__eyebrow">Trusted legal counsel in Ohio</p>
      <h1>Legal strategy that moves as fast as your life.</h1>
      <p class="lead">
        Brian & Sandra Law helps individuals, families, and small businesses resolve legal issues with practical strategy,
        consistent communication, and strong courtroom advocacy.
      </p>
      <div class="home-hero__actions">
        <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Book consultation</button>
        <RouterLink class="hero-button" to="/practice-areas">Explore practice areas</RouterLink>
      </div>
      <div class="home-hero__stats" aria-label="Firm highlights">
        <article class="card home-stat" v-for="stat in quickStats" :key="stat.label">
          <p class="home-stat__label">{{ stat.label }}</p>
          <p class="home-stat__value">{{ stat.value }}</p>
        </article>
      </div>
    </section>

    <section class="content-grid home-panels" data-reveal="left">
      <article class="card home-panel home-panel--lift">
        <h2>Why clients choose us</h2>
        <ul>
          <li v-for="item in valueProps" :key="item">{{ item }}</li>
        </ul>
      </article>
      <article class="card home-panel home-panel--lift">
        <h2>Consultation hours</h2>
        <p>Monday-Friday: 8:30 AM-6:00 PM</p>
        <p>Saturday: By appointment</p>
        <p>Same-week consultations available for urgent matters.</p>
      </article>
    </section>

    <section class="home-practice-grid" data-reveal>
      <article class="card home-practice-card home-panel--lift" v-for="item in practiceAreaHighlights" :key="item.title">
        <p class="home-practice-card__tag">Practice area</p>
        <h3>{{ item.title }}</h3>
        <p>{{ item.body }}</p>
      </article>
    </section>

    <section class="home-parallax-banner" aria-label="Approach" data-reveal="zoom">
      <div class="home-parallax-banner__inner">
        <p class="home-parallax-banner__kicker">Approach</p>
        <h2>Built for clarity, speed, and confidence at every legal milestone.</h2>
      </div>
    </section>

    <section class="home-partners card" data-reveal="right">
      <p class="home-partners__kicker">Partner spotlight</p>
      <h2>Meet the attorneys behind every strategy.</h2>
      <div class="home-partners__grid">
        <RouterLink
          v-for="partner in partnerProfiles"
          :key="partner.slug"
          :to="`/attorneys/${partner.slug}`"
          class="home-partner-card home-partner-card--link"
        >
          <img :src="partner.image" :alt="partner.name" loading="lazy" />
          <div class="home-partner-card__content">
            <h3>{{ partner.name }}</h3>
            <p class="home-partner-card__title">{{ partner.role }}</p>
            <p>{{ partner.shortBio }}</p>
            <span class="home-partner-card__action">View full profile</span>
          </div>
        </RouterLink>
      </div>
    </section>

    <section class="home-process card" data-reveal="zoom">
      <p class="home-process__kicker">What to expect</p>
      <h2>A simple process designed to reduce uncertainty.</h2>
      <ol class="home-process__list">
        <li v-for="step in processSteps" :key="step">{{ step }}</li>
      </ol>
    </section>

    <section class="content-grid home-testimonials" data-reveal>
      <article class="card home-testimonial home-panel--lift" v-for="item in testimonials" :key="item.quote">
        <p class="home-testimonial__quote">"{{ item.quote }}"</p>
        <p class="home-testimonial__source">{{ item.source }}</p>
      </article>
    </section>

    <section class="card marketing-cta" data-reveal="zoom">
      <p class="marketing-cta__kicker">Next step</p>
      <h2>Start your consultation and get a focused legal roadmap.</h2>
      <button type="button" class="hero-button hero-button--primary" @click="openConsultation">Start your consultation</button>
    </section>
  </section>
</template>

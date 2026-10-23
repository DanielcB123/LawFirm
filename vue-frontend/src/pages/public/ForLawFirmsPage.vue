<template>
  <section class="marketing-page">
    <section class="card marketing-hero" data-reveal="zoom">
      <p class="eyebrow">For law firms</p>
      <h1>Launch your firm's public site and secure workspace on one platform.</h1>
      <p class="lead">
        MatterForge onboarding includes review, isolated tenant workspace provisioning, and verified domain routing before go-live.
      </p>
      <div class="hero-stack">
        <RouterLink class="hero-button hero-button--primary" to="/register-firm">Register your firm</RouterLink>
        <RouterLink class="hero-button" to="/workspace/login">Firm sign in</RouterLink>
      </div>
    </section>

    <section class="content-grid marketing-grid" data-reveal="left">
      <article class="card marketing-card">
        <h2>Shared platform, isolated data</h2>
        <p>Each law firm runs on its own tenant schema with host-verified routing and membership-bound authorization.</p>
      </article>
      <article class="card marketing-card">
        <h2>Distinct public experiences</h2>
        <p>Firm-specific layouts and branding are supported without forking workspace authentication or backend services.</p>
      </article>
      <article class="card marketing-card">
        <h2>Domain verification gates</h2>
        <p>TXT ownership proof, routing readiness, and TLS checks are handled as separate audited steps before activation.</p>
      </article>
      <article class="card marketing-card">
        <h2>Operational fallback</h2>
        <p>Every firm retains a canonical fallback tenant URL even when custom domains are active.</p>
      </article>
    </section>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>


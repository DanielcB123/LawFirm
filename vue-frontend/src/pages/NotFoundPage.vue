<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <main class="page">
    <section class="card marketing-card">
      <p class="eyebrow">404</p>
      <h1>Page not found</h1>
      <p>The page you requested does not exist in this route map.</p>
      <RouterLink class="hero-button hero-button--primary" to="/">Back to home</RouterLink>
    </section>
  </main>
</template>

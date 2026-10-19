<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <header class="site-header">
    <div class="site-header__inner">
      <RouterLink class="site-brand-link" to="/">
        <p class="site-brand">Brian & Sandra Law</p>
      </RouterLink>
      <nav class="site-nav" aria-label="Primary">
        <RouterLink class="site-nav__link" to="/" active-class="site-nav__link--active">Home</RouterLink>
        <RouterLink class="site-nav__link" to="/about" active-class="site-nav__link--active">About</RouterLink>
        <RouterLink class="site-nav__link" to="/practice-areas" active-class="site-nav__link--active">
          Practice Areas
        </RouterLink>
        <RouterLink class="site-nav__link" to="/attorneys" active-class="site-nav__link--active">Attorneys</RouterLink>
        <RouterLink class="site-nav__link" to="/contact" active-class="site-nav__link--active">Contact</RouterLink>
        <RouterLink class="site-nav__link" to="/workspace/login" active-class="site-nav__link--active">Sign in</RouterLink>
      </nav>
    </div>
  </header>
</template>

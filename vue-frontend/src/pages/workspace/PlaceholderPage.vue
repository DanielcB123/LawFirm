<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Workspace</p>
      <h1>{{ title }}</h1>
      <p class="muted">
        This page route has been restored so navigation parity exists while we migrate full business logic module-by-module.
      </p>
    </header>
  </section>
</template>

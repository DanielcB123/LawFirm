<script setup lang="ts">
import { useAuthSession } from '../../features/auth/session'

const auth = useAuthSession()
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Dashboard</p>
      <h1>Welcome, {{ auth.actor.value?.name ?? 'Team member' }}</h1>
      <p class="muted">This workspace mirrors the pre-migration route model while Laravel APIs are re-established.</p>
    </header>

    <div class="content-grid">
      <article class="card">
        <h2>Current role</h2>
        <p>{{ auth.actor.value?.role ?? 'Unknown' }}</p>
      </article>
      <article class="card">
        <h2>Permissions</h2>
        <p>{{ auth.actor.value?.permissions?.join(', ') || 'No permissions assigned' }}</p>
      </article>
    </div>
  </section>
</template>

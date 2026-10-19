<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { useAuthSession } from '../features/auth/session'
import SiteFooter from '../components/layout/SiteFooter.vue'

const auth = useAuthSession()
const route = useRoute()
const router = useRouter()

const navItems = [
  { to: '/workspace', label: 'Dashboard' },
  { to: '/workspace/calendar', label: 'Calendar' },
  { to: '/workspace/tasks', label: 'Tasks' },
  { to: '/workspace/matters', label: 'Matters' },
  { to: '/workspace/contacts', label: 'Contacts' },
  { to: '/workspace/activities', label: 'Activities' },
  { to: '/workspace/billing', label: 'Billing' },
  { to: '/workspace/online-payments', label: 'Online payments' },
  { to: '/workspace/accounts', label: 'Accounts' },
  { to: '/workspace/documents', label: 'Documents' },
  { to: '/workspace/communications', label: 'Communications' },
  { to: '/workspace/reports', label: 'Reports' },
  { to: '/workspace/integrations', label: 'App Integrations' },
  { to: '/workspace/settings', label: 'Settings' },
]

const practiceItems = [
  { to: '/workspace/intake', label: 'Intake' },
  { to: '/workspace/consultations', label: 'Consultations' },
]

const actorLabel = computed(() => auth.actor.value?.name ?? 'Unknown user')

async function signOut() {
  auth.signOut()
  await router.push({ name: 'workspace-login', query: { redirect: route.fullPath } })
}
</script>

<template>
  <div class="workspace-shell">
    <aside class="workspace-sidebar">
      <p class="workspace-sidebar__brand">MatterForge Workspace</p>
      <nav class="workspace-nav" aria-label="Workspace">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="workspace-nav__link"
          active-class="workspace-nav__link--active"
          exact-active-class="workspace-nav__link--active"
        >
          {{ item.label }}
        </RouterLink>
      </nav>

      <p class="workspace-sidebar__section">Practice</p>
      <nav class="workspace-nav" aria-label="Practice workflows">
        <RouterLink
          v-for="item in practiceItems"
          :key="item.to"
          :to="item.to"
          class="workspace-nav__link"
          active-class="workspace-nav__link--active"
          exact-active-class="workspace-nav__link--active"
        >
          {{ item.label }}
        </RouterLink>
      </nav>

      <div class="workspace-sidebar__footer">
        <p class="workspace-sidebar__actor">{{ actorLabel }}</p>
        <button type="button" class="workspace-signout" @click="signOut">Sign out</button>
      </div>
    </aside>

    <main class="workspace-main">
      <RouterView />
    </main>
    <SiteFooter class="workspace-footer" />
  </div>
</template>

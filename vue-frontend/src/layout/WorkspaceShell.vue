<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { useAuthSession } from '../features/auth/session'
import SiteFooter from '../components/layout/SiteFooter.vue'
import { useScrollReveal } from '../composables/useScrollReveal'
import { isPlatformHost } from '../features/host/surface'

const auth = useAuthSession()
const route = useRoute()
const router = useRouter()
useScrollReveal(() => route.fullPath)

const firmNavItems = [
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

const platformAdminNavItems = [
  { to: '/workspace/platform/firms', label: 'Law Firm Clients' },
  { to: '/workspace/reports', label: 'Platform Reports' },
  { to: '/workspace/settings', label: 'Platform Settings' },
]

const isPlatformAdminSurface = computed(
  () => isPlatformHost() && auth.actor.value?.platformRole === 'platform_owner',
)

const navItems = computed(() =>
  isPlatformAdminSurface.value ? platformAdminNavItems : firmNavItems,
)

const practiceItems = [
  { to: '/workspace/intake', label: 'Intake' },
  { to: '/workspace/consultations', label: 'Consultations' },
]

const actorLabel = computed(() => auth.actor.value?.name ?? 'Unknown user')
const canSwitchFirm = computed(
  () => !isPlatformAdminSurface.value && isPlatformHost() && (auth.actor.value?.memberships?.length ?? 0) > 1,
)

async function signOut() {
  auth.signOut()
  await router.push({ name: 'workspace-login', query: { redirect: route.fullPath } })
}
</script>

<template>
  <div class="workspace-shell">
    <aside class="workspace-sidebar">
      <p class="workspace-sidebar__brand">
        {{ isPlatformAdminSurface ? 'MatterForge Platform Admin' : 'MatterForge Workspace' }}
      </p>
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

      <p v-if="!isPlatformAdminSurface" class="workspace-sidebar__section">Practice</p>
      <nav v-if="!isPlatformAdminSurface" class="workspace-nav" aria-label="Practice workflows">
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
        <RouterLink
          v-if="canSwitchFirm"
          to="/workspace/select-firm"
          class="workspace-nav__link workspace-nav__link--switch"
        >
          Switch firm
        </RouterLink>
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

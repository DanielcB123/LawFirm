import { createRouter, createWebHistory } from 'vue-router'
import PublicShell from '../layout/PublicShell.vue'
import WorkspaceShell from '../layout/WorkspaceShell.vue'
import { useAuthSession } from '../features/auth/session'
import HomePage from '../pages/public/HomePage.vue'
import AboutPage from '../pages/public/AboutPage.vue'
import PracticeAreasPage from '../pages/public/PracticeAreasPage.vue'
import AttorneysPage from '../pages/public/AttorneysPage.vue'
import AttorneyProfilePage from '../pages/public/AttorneyProfilePage.vue'
import ContactPage from '../pages/public/ContactPage.vue'
import ForLawFirmsPage from '../pages/public/ForLawFirmsPage.vue'
import RegisterFirmPage from '../pages/public/RegisterFirmPage.vue'
import OnboardingStatusPage from '../pages/public/OnboardingStatusPage.vue'
import NotFoundPage from '../pages/NotFoundPage.vue'
import WorkspaceLoginPage from '../pages/workspace/WorkspaceLoginPage.vue'
import WorkspaceHomePage from '../pages/workspace/WorkspaceHomePage.vue'
import FirmSelectorPage from '../pages/workspace/FirmSelectorPage.vue'
import ContactsPage from '../pages/workspace/ContactsPage.vue'
import IntakePage from '../pages/workspace/IntakePage.vue'
import ConsultationsQueuePage from '../pages/workspace/ConsultationsQueuePage.vue'
import WorkspaceCalendarPage from '../pages/workspace/WorkspaceCalendarPage.vue'
import MattersPage from '../pages/workspace/MattersPage.vue'
import TasksPage from '../pages/workspace/TasksPage.vue'
import PlatformFirmsPage from '../pages/workspace/PlatformFirmsPage.vue'
import WorkspaceUsersPage from '../pages/workspace/WorkspaceUsersPage.vue'
import PlaceholderPage from '../pages/workspace/PlaceholderPage.vue'
import { isPlatformHost, isTenantHost } from '../features/host/surface'

const routes = [
  {
    path: '/',
    component: PublicShell,
    children: [
      { path: '', name: 'home', component: HomePage },
      { path: 'about', name: 'about', component: AboutPage },
      { path: 'practice-areas', name: 'practice-areas', component: PracticeAreasPage },
      { path: 'attorneys', name: 'attorneys', component: AttorneysPage },
      { path: 'attorneys/:slug', name: 'attorney-profile', component: AttorneyProfilePage },
      { path: 'contact', name: 'contact', component: ContactPage },
      { path: 'for-law-firms', name: 'for-law-firms', component: ForLawFirmsPage },
      { path: 'register-firm', name: 'register-firm', component: RegisterFirmPage },
      { path: 'onboarding/status', name: 'onboarding-status', component: OnboardingStatusPage, meta: { requiresAuth: true } },
      { path: 'workspace/login', name: 'workspace-login', component: WorkspaceLoginPage, meta: { guestOnly: true } },
    ],
  },
  {
    path: '/workspace',
    component: WorkspaceShell,
    meta: { requiresAuth: true },
    children: [
      { path: '', name: 'workspace-home', component: WorkspaceHomePage },
      { path: 'select-firm', name: 'workspace-select-firm', component: FirmSelectorPage },
      { path: 'contacts', name: 'workspace-contacts', component: ContactsPage },
      { path: 'intake', name: 'workspace-intake', component: IntakePage },
      { path: 'consultations', name: 'workspace-consultations', component: ConsultationsQueuePage },
      { path: 'calendar', name: 'workspace-calendar', component: WorkspaceCalendarPage },
      { path: 'tasks', name: 'workspace-tasks', component: TasksPage },
      { path: 'matters', name: 'workspace-matters', component: MattersPage },
      { path: 'activities', name: 'workspace-activities', component: PlaceholderPage, props: { title: 'Activities' } },
      { path: 'billing', name: 'workspace-billing', component: PlaceholderPage, props: { title: 'Billing' } },
      {
        path: 'online-payments',
        name: 'workspace-online-payments',
        component: PlaceholderPage,
        props: { title: 'Online Payments' },
      },
      { path: 'accounts', name: 'workspace-accounts', component: PlaceholderPage, props: { title: 'Accounts' } },
      { path: 'documents', name: 'workspace-documents', component: PlaceholderPage, props: { title: 'Documents' } },
      {
        path: 'communications',
        name: 'workspace-communications',
        component: PlaceholderPage,
        props: { title: 'Communications' },
      },
      { path: 'reports', name: 'workspace-reports', component: PlaceholderPage, props: { title: 'Reports' } },
      {
        path: 'integrations',
        name: 'workspace-integrations',
        component: PlaceholderPage,
        props: { title: 'App Integrations' },
      },
      { path: 'settings', name: 'workspace-settings', component: WorkspaceUsersPage },
      { path: 'platform/firms', name: 'workspace-platform-firms', component: PlatformFirmsPage },
    ],
  },
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundPage },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})

router.beforeEach(async (to) => {
  const auth = useAuthSession()
  await auth.ensureSessionLoaded()
  const memberships = auth.actor.value?.memberships ?? []
  const isPlatformOwner = auth.actor.value?.platformRole === 'platform_owner'

  if (to.meta.requiresAuth && !auth.isAuthenticated.value) {
    if (isTenantHost()) {
      return { name: 'home' }
    }

    return { name: 'workspace-login', query: { redirect: to.fullPath } }
  }

  if (
    to.meta.requiresAuth &&
    auth.isAuthenticated.value &&
    isPlatformHost() &&
    !isPlatformOwner &&
    to.name !== 'workspace-select-firm' &&
    memberships.length > 1 &&
    !auth.selectedFirmId.value
  ) {
    return { name: 'workspace-select-firm' }
  }

  if (to.name === 'workspace-select-firm' && memberships.length <= 1) {
    return { name: 'workspace-home' }
  }

  if (
    to.meta.requiresAuth &&
    auth.isAuthenticated.value &&
    isPlatformHost() &&
    isPlatformOwner &&
    to.name === 'workspace-home'
  ) {
    return { name: 'workspace-platform-firms' }
  }

  if (to.meta.guestOnly && auth.isAuthenticated.value) {
    if (isPlatformHost() && isPlatformOwner) {
      return { name: 'workspace-platform-firms' }
    }

    return { name: 'workspace-home' }
  }

  return true
})

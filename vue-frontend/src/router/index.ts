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
import NotFoundPage from '../pages/NotFoundPage.vue'
import WorkspaceLoginPage from '../pages/workspace/WorkspaceLoginPage.vue'
import WorkspaceHomePage from '../pages/workspace/WorkspaceHomePage.vue'
import ContactsPage from '../pages/workspace/ContactsPage.vue'
import IntakePage from '../pages/workspace/IntakePage.vue'
import ConsultationsQueuePage from '../pages/workspace/ConsultationsQueuePage.vue'
import WorkspaceCalendarPage from '../pages/workspace/WorkspaceCalendarPage.vue'
import MattersPage from '../pages/workspace/MattersPage.vue'
import TasksPage from '../pages/workspace/TasksPage.vue'
import PlaceholderPage from '../pages/workspace/PlaceholderPage.vue'

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
    ],
  },
  {
    path: '/workspace/login',
    name: 'workspace-login',
    component: WorkspaceLoginPage,
    meta: { guestOnly: true },
  },
  {
    path: '/workspace',
    component: WorkspaceShell,
    meta: { requiresAuth: true },
    children: [
      { path: '', name: 'workspace-home', component: WorkspaceHomePage },
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
      { path: 'settings', name: 'workspace-settings', component: PlaceholderPage, props: { title: 'Settings' } },
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

  if (to.meta.requiresAuth && !auth.isAuthenticated.value) {
    return { name: 'workspace-login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guestOnly && auth.isAuthenticated.value) {
    return { name: 'workspace-home' }
  }

  return true
})

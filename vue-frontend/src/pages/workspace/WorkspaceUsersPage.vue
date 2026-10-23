<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ApiClientError } from '../../lib/apiClient'
import { useAuthSession } from '../../features/auth/session'
import { createTeamMember, listTeamMembers, updateTeamMemberRole, type TeamMember } from '../../features/team/api'

const auth = useAuthSession()
const members = ref<TeamMember[]>([])
const roles = ref<string[]>([])
const rolePermissions = ref<Record<string, string[]>>({})
const error = ref<string | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const canManageUsers = computed(() => auth.hasPermission('users.manage'))

const createForm = ref({
  name: '',
  email: '',
  password: '',
  role: 'Attorney',
})

async function loadMembers() {
  const token = auth.token.value
  if (!token || !canManageUsers.value) {
    members.value = []
    return
  }

  isLoading.value = true
  error.value = null
  try {
    const payload = await listTeamMembers(token)
    roles.value = payload.roles
    rolePermissions.value = payload.rolePermissions
    members.value = payload.members

    if (roles.value.length > 0 && !roles.value.includes(createForm.value.role)) {
      createForm.value.role = roles.value[0]
    }
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to load team members.'
  } finally {
    isLoading.value = false
  }
}

async function createUser() {
  const token = auth.token.value
  if (!token || !canManageUsers.value) return

  isSaving.value = true
  error.value = null
  try {
    await createTeamMember(token, {
      name: createForm.value.name.trim(),
      email: createForm.value.email.trim(),
      password: createForm.value.password,
      role: createForm.value.role,
    })
    createForm.value = {
      name: '',
      email: '',
      password: '',
      role: roles.value[0] ?? 'Attorney',
    }
    await loadMembers()
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to create team member.'
  } finally {
    isSaving.value = false
  }
}

async function changeRole(member: TeamMember, role: string) {
  const token = auth.token.value
  if (!token || !canManageUsers.value) return

  error.value = null
  try {
    await updateTeamMemberRole(token, member.userId, role)
    member.membershipRole = role
    member.permissions = rolePermissions.value[role] ?? []
  } catch (err) {
    error.value = err instanceof ApiClientError ? err.message : 'Unable to update team role.'
  }
}

onMounted(() => {
  void loadMembers()
})
</script>

<template>
  <section class="workspace-page">
    <header class="workspace-page__header">
      <p class="eyebrow">Settings</p>
      <h1>Team access</h1>
      <p class="muted">
        Partner users can create firm accounts and assign role-based workspace permissions.
      </p>
    </header>

    <p v-if="!canManageUsers" class="error-message">
      Only firm partners can manage team users and permissions.
    </p>

    <template v-else>
      <div class="content-grid">
        <article class="card workspace-form-card">
          <h2>Add team user</h2>
          <form class="workspace-form" @submit.prevent="createUser">
            <label>
              Full name
              <input v-model="createForm.name" type="text" required />
            </label>
            <label>
              Login email (username)
              <input v-model="createForm.email" type="email" required />
            </label>
            <label>
              Temporary password
              <input v-model="createForm.password" type="password" minlength="8" required />
            </label>
            <label>
              Role
              <select v-model="createForm.role">
                <option v-for="role in roles" :key="role" :value="role">{{ role }}</option>
              </select>
            </label>
            <button type="submit" :disabled="isSaving">
              {{ isSaving ? 'Saving...' : 'Create user' }}
            </button>
          </form>
        </article>

        <article class="card">
          <h2>Role permissions</h2>
          <p class="muted">
            Page access and disabled inputs are controlled by these permissions plus API enforcement.
          </p>
          <ul class="record-list">
            <li v-for="role in roles" :key="`role-${role}`">
              <strong>{{ role }}</strong>
              <span>{{ (rolePermissions[role] ?? []).join(', ') || 'No permissions' }}</span>
            </li>
          </ul>
        </article>
      </div>

      <article class="card">
        <h2>Firm users</h2>
        <p v-if="isLoading" class="muted">Loading team members...</p>
        <ul class="record-list">
          <li v-for="member in members" :key="member.membershipId">
            <strong>{{ member.name }}</strong>
            <span>{{ member.email }}</span>
            <label>
              Role
              <select :value="member.membershipRole" @change="changeRole(member, ($event.target as HTMLSelectElement).value)">
                <option v-for="role in roles" :key="`member-${member.membershipId}-${role}`" :value="role">{{ role }}</option>
              </select>
            </label>
            <span>{{ member.permissions.join(', ') || 'No permissions' }}</span>
          </li>
        </ul>
      </article>
    </template>

    <p v-if="error" class="error-message">{{ error }}</p>
  </section>
</template>


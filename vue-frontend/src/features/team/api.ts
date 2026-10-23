import { getJson, postJson, putJson } from '../../lib/apiClient'

export type TeamMember = {
  membershipId: string
  userId: number
  name: string
  email: string
  membershipRole: string
  permissions: string[]
}

export type TeamAccessPayload = {
  roles: string[]
  rolePermissions: Record<string, string[]>
  members: TeamMember[]
}

export async function listTeamMembers(token: string): Promise<TeamAccessPayload> {
  return getJson<TeamAccessPayload>('/tenant/users', token)
}

export async function createTeamMember(
  token: string,
  payload: { name: string; email: string; password: string; role: string },
): Promise<TeamMember> {
  return postJson<TeamMember, typeof payload>('/tenant/users', payload, token)
}

export async function updateTeamMemberRole(
  token: string,
  userId: number,
  role: string,
): Promise<{ membershipId: string; userId: number; membershipRole: string; permissions: string[] }> {
  return putJson(`/tenant/users/${userId}/membership`, { role }, token)
}


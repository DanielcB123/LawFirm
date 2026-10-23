export type FirmMembershipSummary = {
  firmId: string
  displayName: string
  lifecycleStatus: string
  role: string
  preferredHost?: string | null
  fallbackHost?: string | null
}

export type ActorIdentity = {
  id: number
  name: string
  email: string
  role: string
  permissions: string[]
  platformRole?: 'platform_owner' | 'platform_ops' | null
  platformPermissions?: string[]
  isPlatformAdministrator?: boolean
  memberships?: FirmMembershipSummary[]
}

export type LoginResponse = {
  access_token: string
  token_type: string
  user: ActorIdentity
}

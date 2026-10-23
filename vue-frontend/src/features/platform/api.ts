import { getJson, postJson, putJson } from '../../lib/apiClient'

export type PlatformFirm = {
  id: string
  legalName: string
  displayName: string
  slug: string
  lifecycleStatus: string
  provisioningStatus: string
  provisioningFailureReason: string | null
  schemaVersion: string | null
  publicSiteKey: 'default_classic' | 'modern_feature'
  domains: Array<{
    id: string
    hostname: string
    isPrimary: boolean
    verifiedAt: string | null
    revokedAt: string | null
    verificationLastError: string | null
  }>
  memberships: Array<{
    userId: number
    userName: string | null
    userEmail: string | null
    role: string
  }>
  onboarding: {
    hasApplication: boolean
    applicantUserId: number | null
    revision: number | null
    approvalCount: number
    requiredApprovals: number
    verificationStatuses: Array<{
      type: string
      status: string
      method: string
      reviewedAt: string | null
    }>
  }
}

type PlatformFirmListResponse = {
  data: PlatformFirm[]
  meta: {
    currentPage: number
    lastPage: number
    perPage: number
    total: number
  }
}

export async function listPlatformFirms(token: string, status = '', search = ''): Promise<PlatformFirmListResponse> {
  const params = new URLSearchParams()
  if (status) {
    params.set('status', status)
  }
  if (search.trim() !== '') {
    params.set('search', search.trim())
  }

  const query = params.size > 0 ? `?${params.toString()}` : ''
  return getJson<PlatformFirmListResponse>(`/platform/firms${query}`, token)
}

export async function createPlatformFirm(
  token: string,
  payload: { legalName: string; displayName: string },
): Promise<PlatformFirm> {
  return postJson<PlatformFirm, { legalName: string; displayName: string }>('/platform/firms', payload, token)
}

export async function reviewPlatformFirm(
  token: string,
  firmId: string,
  payload: { decision: 'approve' | 'reject'; reason: string; evidenceReferences: string[] },
): Promise<PlatformFirm> {
  return postJson<PlatformFirm, typeof payload>(`/platform/firms/${firmId}/review`, payload, token)
}

export async function provisionPlatformFirm(token: string, firmId: string): Promise<PlatformFirm> {
  return postJson<PlatformFirm, Record<string, never>>(`/platform/firms/${firmId}/provision`, {}, token)
}

export async function suspendPlatformFirm(token: string, firmId: string, reason: string): Promise<PlatformFirm> {
  return postJson<PlatformFirm, { reason: string }>(`/platform/firms/${firmId}/suspend`, { reason }, token)
}

export async function reactivatePlatformFirm(token: string, firmId: string, reason: string): Promise<PlatformFirm> {
  return postJson<PlatformFirm, { reason: string }>(`/platform/firms/${firmId}/reactivate`, { reason }, token)
}

export async function addPlatformFirmMembership(
  token: string,
  firmId: string,
  payload: { userId: number; role: string },
): Promise<unknown> {
  return postJson(`/platform/firms/${firmId}/memberships`, payload, token)
}

export async function claimPlatformFirmDomain(
  token: string,
  firmId: string,
  payload: { hostname: string; primary: boolean },
): Promise<unknown> {
  return postJson(`/platform/firms/${firmId}/domains`, payload, token)
}

export async function verifyPlatformFirmDomain(token: string, firmId: string, domainId: string): Promise<unknown> {
  return postJson(`/platform/firms/${firmId}/domains/${domainId}/verify`, {}, token)
}

export async function reissuePlatformFirmDomainChallenge(token: string, firmId: string, domainId: string): Promise<unknown> {
  return postJson(`/platform/firms/${firmId}/domains/${domainId}/reissue-challenge`, {}, token)
}

export async function revokePlatformFirmDomain(
  token: string,
  firmId: string,
  domainId: string,
  reason: string,
): Promise<unknown> {
  return postJson(`/platform/firms/${firmId}/domains/${domainId}/revoke`, { reason }, token)
}

export async function savePlatformFirmBranding(
  token: string,
  firmId: string,
  payload: {
    logoPath: string | null
    faviconPath: string | null
    colors: { primary: string; secondary: string; accent: string }
    typography: 'system-sans' | 'system-serif'
    contactEmail: string | null
    contactPhone: string | null
    contactAddress: string | null
    publicCopy: string
    publicSiteKey: 'default_classic' | 'modern_feature'
    enabledModules: string[]
  },
): Promise<unknown> {
  return putJson(`/platform/firms/${firmId}/branding`, payload, token)
}

export async function savePlatformFirmVerification(
  token: string,
  firmId: string,
  payload: { verificationType: string; status: string; method: 'manual' | 'provider'; notes?: string | null },
): Promise<PlatformFirm> {
  return postJson(`/platform/firms/${firmId}/verifications`, payload, token)
}

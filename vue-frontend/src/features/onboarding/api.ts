import { getJson, postJson, putJson } from '../../lib/apiClient'
import type { LoginResponse } from '../../types/auth'

export type OnboardingVerificationStatus = {
  type: string
  label: string
  status: string
  method: string
  reviewedAt: string | null
}

export type OnboardingApplicationStatus = {
  id: string
  firmId: string
  firmName: string
  revision: number
  stage:
    | 'pending_review'
    | 'awaiting_second_approval'
    | 'rejected'
    | 'provisioning'
    | 'provisioning_failed'
    | 'active'
  lifecycleStatus: string
  provisioningStatus: string
  provisioningFailureReason: string | null
  approvalCount: number
  requiredApprovals: number
  preferredHost: string | null
  fallbackHost: string | null
  ownerMembershipReady: boolean
  verifications: OnboardingVerificationStatus[]
}

export type OnboardingStatusResponse = {
  hasApplication: boolean
  fastTrackEnabled: boolean
  developmentNotice: string | null
  application?: OnboardingApplicationStatus
}

type RegisterFirmRequest = {
  firmName: string
  name: string
  email: string
  password: string
  password_confirmation: string
  businessPhone?: string | null
}

type RegisterFirmResponse = LoginResponse & {
  applicationId: string
}

export type FirmNameAvailabilityResponse = {
  firmName: string
  slug: string
  available: boolean
  suggestions: string[]
  message: string
}

export async function registerFirm(payload: RegisterFirmRequest): Promise<RegisterFirmResponse> {
  return postJson<RegisterFirmResponse, RegisterFirmRequest>('/onboarding/register', payload)
}

export async function checkFirmNameAvailability(firmName: string): Promise<FirmNameAvailabilityResponse> {
  const params = new URLSearchParams({ firmName })
  return getJson<FirmNameAvailabilityResponse>(`/onboarding/firm-name-availability?${params.toString()}`)
}

export async function submitFirmApplication(
  token: string,
  payload: { firmName: string; applicantName?: string; applicantEmail?: string; businessPhone?: string | null },
): Promise<{ applicationId: string; firmId: string }> {
  return postJson('/onboarding/applications', payload, token)
}

export async function getOnboardingStatus(token: string): Promise<OnboardingStatusResponse> {
  return getJson<OnboardingStatusResponse>('/onboarding/applications/current', token)
}

export async function updateOnboardingApplication(
  token: string,
  payload: { firmName: string; businessPhone?: string | null },
): Promise<OnboardingStatusResponse> {
  return putJson('/onboarding/applications/current', payload, token)
}

export async function runFastTrackOnboarding(token: string): Promise<OnboardingStatusResponse> {
  return postJson('/onboarding/applications/current/fast-track', {}, token)
}

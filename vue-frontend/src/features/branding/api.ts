import { getJson } from '../../lib/apiClient'
import { firmExtensionLoaders } from '../../firms/extensions'
import { platformBranding, type PublicBranding } from './types'
import { ref } from 'vue'

const hexColor = /^#[0-9A-Fa-f]{6}$/
const assetPath = /^\/storage\/branding\/[a-z0-9-]{1,80}\.(png|webp|svg|ico)$/

function plainText(value: unknown, fallback: string): string {
  if (typeof value !== 'string') {
    return fallback
  }

  const stripped = value.replace(/[<>]/g, '').trim()
  return stripped === '' ? fallback : stripped
}

function color(value: unknown, fallback: string): string {
  return typeof value === 'string' && hexColor.test(value) ? value : fallback
}

function asset(value: unknown): string | null {
  return typeof value === 'string' && assetPath.test(value) ? value : null
}

export function sanitizePublicBranding(payload: Partial<PublicBranding> | null): PublicBranding {
  const typography = payload?.typography === 'system-serif' ? 'system-serif' : 'system-sans'
  const modules = Array.isArray(payload?.enabledModules)
    ? payload.enabledModules.filter((module) => module === 'public_site' || module === 'consultations')
    : platformBranding.enabledModules

  return {
    source: payload?.source === 'firm' ? 'firm' : 'platform',
    displayName: plainText(payload?.displayName, platformBranding.displayName),
    publicCopy: plainText(payload?.publicCopy, platformBranding.publicCopy),
    contactEmail: typeof payload?.contactEmail === 'string' && payload.contactEmail.includes('@') ? payload.contactEmail : null,
    contactPhone: plainText(payload?.contactPhone, '') || null,
    contactAddress: plainText(payload?.contactAddress, ''),
    logoPath: asset(payload?.logoPath),
    faviconPath: asset(payload?.faviconPath),
    colors: {
      primary: color(payload?.colors?.primary, platformBranding.colors.primary),
      secondary: color(payload?.colors?.secondary, platformBranding.colors.secondary),
      accent: color(payload?.colors?.accent, platformBranding.colors.accent),
    },
    typography,
    publicSiteKey:
      payload?.publicSiteKey === 'modern_feature' || payload?.publicSiteKey === 'default_classic'
        ? payload.publicSiteKey
        : platformBranding.publicSiteKey,
    enabledModules: modules,
  }
}

let cachedBranding: PublicBranding | null = null
let pendingBranding: Promise<PublicBranding> | null = null
const brandingState = ref<PublicBranding>(platformBranding)

export function loadPublicBranding(): Promise<PublicBranding> {
  if (cachedBranding) {
    return Promise.resolve(cachedBranding)
  }

  if (!pendingBranding) {
    pendingBranding = getJson<PublicBranding>('/public/branding')
      .then((payload) => {
        cachedBranding = sanitizePublicBranding(payload)
        brandingState.value = cachedBranding
        return cachedBranding
      })
      .finally(() => {
        pendingBranding = null
      })
  }

  return pendingBranding
}

export function applyPublicBranding(branding: PublicBranding): void {
  brandingState.value = branding
  const root = document.documentElement
  document.title = branding.source === 'firm' ? `${branding.displayName} | MatterForge` : 'MatterForge'

  if (branding.source === 'firm') {
    root.style.setProperty('--firm-primary', branding.colors.primary)
    root.style.setProperty('--firm-accent', branding.colors.accent)
    root.dataset.firmTypography = branding.typography
  } else {
    root.style.removeProperty('--firm-primary')
    root.style.removeProperty('--firm-accent')
    delete root.dataset.firmTypography
  }
  root.dataset.publicSiteKey = branding.publicSiteKey

  const icon = asset(branding.faviconPath)
  if (!icon) {
    return
  }

  let link = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
  if (!link) {
    link = document.createElement('link')
    link.rel = 'icon'
    document.head.append(link)
  }
  link.href = icon
}

export function registeredFirmExtensionCount(): number {
  return Object.keys(firmExtensionLoaders).length
}

export function usePublicBrandingState() {
  return brandingState
}

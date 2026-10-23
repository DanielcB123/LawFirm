export type PublicBranding = {
  source: 'platform' | 'firm'
  displayName: string
  publicCopy: string
  contactEmail: string | null
  contactPhone: string | null
  contactAddress: string
  logoPath: string | null
  faviconPath: string | null
  colors: {
    primary: string
    secondary: string
    accent: string
  }
  typography: 'system-sans' | 'system-serif'
  publicSiteKey: 'default_classic' | 'modern_feature'
  enabledModules: string[]
}

export const platformBranding: PublicBranding = {
  source: 'platform',
  displayName: 'MatterForge',
  publicCopy: 'All-in-one legal operations platform for modern law firms.',
  contactEmail: null,
  contactPhone: '(614) 555-0101',
  contactAddress: 'MatterForge Platform',
  logoPath: null,
  faviconPath: null,
  colors: {
    primary: '#2F1F1A',
    secondary: '#4D3F36',
    accent: '#5D1F2F',
  },
  typography: 'system-sans',
  publicSiteKey: 'default_classic',
  enabledModules: ['public_site', 'consultations'],
}

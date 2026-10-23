/**
 * Code-registered firm UI extensions.
 * Public branding JSON cannot name or load one of these modules.
 */
export type FirmPublicSiteKey = 'default_classic' | 'modern_feature'

export type FirmExtensionDefinition = {
  key: FirmPublicSiteKey
  label: string
  shellClass: string
}

export const builtInFirmExtensions: Record<FirmPublicSiteKey, FirmExtensionDefinition> = {
  default_classic: {
    key: 'default_classic',
    label: 'Classic Counsel',
    shellClass: 'public-shell--classic',
  },
  modern_feature: {
    key: 'modern_feature',
    label: 'Modern Feature',
    shellClass: 'public-shell--modern-feature',
  },
}

export const firmExtensionLoaders: Record<string, () => Promise<unknown>> = {}

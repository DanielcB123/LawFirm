const platformHosts = (import.meta.env.VITE_PLATFORM_HOSTS ?? 'localhost,127.0.0.1')
  .split(',')
  .map((entry: string) => entry.trim().toLowerCase())
  .filter((entry: string) => entry.length > 0)

const appHosts = (import.meta.env.VITE_APP_HOSTS ?? 'app.localhost')
  .split(',')
  .map((entry: string) => entry.trim().toLowerCase())
  .filter((entry: string) => entry.length > 0)

function currentHost(): string {
  if (typeof window === 'undefined') {
    return ''
  }

  return window.location.hostname.toLowerCase()
}

export function isPlatformHost(host = currentHost()): boolean {
  return platformHosts.includes(host)
}

export function isAppHost(host = currentHost()): boolean {
  return appHosts.includes(host)
}

export function isTenantHost(host = currentHost()): boolean {
  return host !== '' && !isPlatformHost(host) && !isAppHost(host)
}


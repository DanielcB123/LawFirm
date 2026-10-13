// Only VITE_-prefixed variables reach the browser bundle; never put secrets in them.
export const env = {
  // Empty in development: requests go to /api and the Vite proxy forwards them to the API.
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, ""),
} as const;

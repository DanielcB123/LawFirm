const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080/api'

export class ApiClientError extends Error {
  statusCode: number

  constructor(message: string, statusCode: number) {
    super(message)
    this.statusCode = statusCode
  }
}

function buildHeaders(token?: string, headers?: HeadersInit): HeadersInit {
  const merged: Record<string, string> = {
    Accept: 'application/json',
  }

  if (headers && !Array.isArray(headers)) {
    Object.assign(merged, headers as Record<string, string>)
  }

  if (token) {
    merged.Authorization = `Bearer ${token}`
  }

  return merged
}

export async function getJson<T>(path: string, token?: string): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: buildHeaders(token),
  })

  if (!response.ok) {
    throw await parseApiError(response)
  }

  return (await response.json()) as T
}

export async function postJson<TResponse, TRequest>(
  path: string,
  body: TRequest,
  token?: string,
): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: buildHeaders(token, { 'Content-Type': 'application/json' }),
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw await parseApiError(response)
  }

  return (await response.json()) as TResponse
}

async function parseApiError(response: Response): Promise<ApiClientError> {
  try {
    const payload = (await response.json()) as { message?: string }
    if (payload.message) {
      return new ApiClientError(payload.message, response.status)
    }
  } catch {
    // Ignore JSON parsing failures and fallback to status text below.
  }

  return new ApiClientError(`Request failed (${response.status})`, response.status)
}

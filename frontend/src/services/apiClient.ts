type ApiErrorEnvelope = {
  correlationId: string;
  error: {
    code: string;
    message: string;
    validationErrors?: Record<string, string[]>;
  };
};

export class ApiClientError extends Error {
  public readonly statusCode: number;
  public readonly correlationId?: string;
  public readonly validationErrors?: Record<string, string[]>;

  constructor(
    message: string,
    statusCode: number,
    correlationId?: string,
    validationErrors?: Record<string, string[]>,
  ) {
    super(message);
    this.statusCode = statusCode;
    this.correlationId = correlationId;
    this.validationErrors = validationErrors;
  }
}

function buildHeaders(token?: string, headers?: HeadersInit): HeadersInit {
  const mergedHeaders: Record<string, string> = {
    Accept: "application/json",
    ...(headers as Record<string, string> | undefined),
  };

  if (token) {
    mergedHeaders.Authorization = `Bearer ${token}`;
  }

  return mergedHeaders;
}

export async function getJson<T>(path: string, init?: RequestInit, token?: string): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: buildHeaders(token, init?.headers),
  });

  if (!response.ok) {
    await throwApiError(response);
  }

  return (await response.json()) as T;
}

export async function postJson<TResponse, TRequest>(
  path: string,
  body: TRequest,
  token?: string,
): Promise<TResponse> {
  const response = await fetch(path, {
    method: "POST",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    await throwApiError(response);
  }

  return (await response.json()) as TResponse;
}

export async function putJson<TResponse, TRequest>(
  path: string,
  body: TRequest,
  token?: string,
): Promise<TResponse> {
  const response = await fetch(path, {
    method: "PUT",
    headers: buildHeaders(token, { "Content-Type": "application/json" }),
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    await throwApiError(response);
  }

  return (await response.json()) as TResponse;
}

export async function deleteRequest(path: string, token?: string): Promise<void> {
  const response = await fetch(path, {
    method: "DELETE",
    headers: buildHeaders(token),
  });

  if (!response.ok && response.status !== 204) {
    await throwApiError(response);
  }
}

async function throwApiError(response: Response): Promise<never> {
  let envelope: ApiErrorEnvelope | null = null;
  try {
    envelope = (await response.json()) as ApiErrorEnvelope;
  } catch {
    // Non-json errors from reverse proxies or middleware still map to a generic API error.
  }

  if (envelope?.error?.message) {
    throw new ApiClientError(
      envelope.error.message,
      response.status,
      envelope.correlationId,
      envelope.error.validationErrors,
    );
  }

  throw new ApiClientError(`Request failed: ${response.status} ${response.statusText}`, response.status);
}

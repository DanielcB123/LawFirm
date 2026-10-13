import { getJson, postJson } from "../../lib/apiClient";
import type { ClientDocument, ClientMatter, SubmitClientDocumentPayload } from "./types";

// Client-only routes; staff routes under /api/matters-v2 reject the Client role.
const basePath = "/api/portal/matters";

export async function getClientMatters(token: string): Promise<ClientMatter[]> {
  return getJson<ClientMatter[]>(basePath, undefined, token);
}

export async function getClientMatter(token: string, matterId: string): Promise<ClientMatter> {
  return getJson<ClientMatter>(`${basePath}/${matterId}`, undefined, token);
}

export async function getClientDocuments(token: string, matterId: string): Promise<ClientDocument[]> {
  return getJson<ClientDocument[]>(`${basePath}/${matterId}/documents`, undefined, token);
}

export async function submitClientDocument(
  token: string,
  matterId: string,
  payload: SubmitClientDocumentPayload,
): Promise<ClientDocument> {
  return postJson<ClientDocument, SubmitClientDocumentPayload>(`${basePath}/${matterId}/documents`, payload, token);
}

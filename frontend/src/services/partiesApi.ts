import { getJson, postJson } from "./apiClient";
import type { Party } from "../types/parties";

export type CreatePartyPayload = {
  displayName: string;
  partyType: "Person" | "Organization";
  primaryEmail?: string;
  primaryPhone?: string;
  isRestricted: boolean;
};

export async function searchParties(token: string, query = ""): Promise<Party[]> {
  const encoded = encodeURIComponent(query);
  return getJson<Party[]>(`/api/parties?query=${encoded}`, undefined, token);
}

export async function createParty(token: string, payload: CreatePartyPayload): Promise<Party> {
  return postJson<Party, CreatePartyPayload>("/api/parties", payload, token);
}

export async function addPartyAlias(
  token: string,
  partyId: string,
  payload: { alias: string; aliasType: string },
): Promise<void> {
  await postJson<{ id: string; alias: string; aliasType: string }, { alias: string; aliasType: string }>(
    `/api/parties/${partyId}/aliases`,
    payload,
    token,
  );
}

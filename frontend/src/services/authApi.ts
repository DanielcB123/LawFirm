import { getJson, postJson } from "./apiClient";
import type { ActorProfile, IdentityResponse, LoginResponse } from "../types/auth";

type LoginRequest = {
  email: string;
  password: string;
};

export async function loginWithPassword(email: string, password: string): Promise<LoginResponse> {
  return postJson<LoginResponse, LoginRequest>("/api/auth/login", { email, password });
}

export async function getActorProfile(token: string): Promise<ActorProfile> {
  const response = await getJson<IdentityResponse>("/api/identity/me", undefined, token);
  return {
    id: response.actorId,
    email: response.email,
    displayName: response.displayName,
    role: response.role,
    permissions: response.permissions,
  };
}

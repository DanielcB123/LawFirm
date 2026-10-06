import { getJson, postJson } from "./apiClient";
import type { ConsultationRequestItem, ConsultationSlot } from "../types/consultations";

type CreateConsultationPayload = {
  fullName: string;
  email: string;
  phone?: string;
  practiceArea: string;
  message?: string;
  preferredAtUtc: string;
  timeZone: string;
};

type UpdateConsultationStatusPayload = {
  status?: string;
  assignedToActorId?: string;
  assignedToDisplayName?: string;
  internalNotes?: string;
};

type GetAvailableConsultationSlotsOptions = {
  days?: number;
  startDate?: string;
};

export async function getAvailableConsultationSlots(
  options: number | GetAvailableConsultationSlotsOptions = 14,
): Promise<ConsultationSlot[]> {
  const normalized = typeof options === "number" ? { days: options } : options;
  const params = new URLSearchParams();
  if (normalized.days) {
    params.set("days", `${normalized.days}`);
  }
  if (normalized.startDate) {
    params.set("startDate", normalized.startDate);
  }

  const query = params.size > 0 ? `?${params.toString()}` : "";
  return getJson<ConsultationSlot[]>(`/api/consultations/available-slots${query}`);
}

export async function createConsultationRequest(payload: CreateConsultationPayload): Promise<ConsultationRequestItem> {
  return postJson<ConsultationRequestItem, CreateConsultationPayload>("/api/consultations", payload);
}

export async function getConsultationQueue(token: string, status?: string): Promise<ConsultationRequestItem[]> {
  const suffix = status ? `?status=${encodeURIComponent(status)}` : "";
  return getJson<ConsultationRequestItem[]>(`/api/consultations${suffix}`, undefined, token);
}

export async function updateConsultationStatus(
  token: string,
  consultationId: string,
  payload: UpdateConsultationStatusPayload,
): Promise<ConsultationRequestItem> {
  return postJson<ConsultationRequestItem, UpdateConsultationStatusPayload>(
    `/api/consultations/${consultationId}/status`,
    payload,
    token,
  );
}

import { getJson, postJson } from "./apiClient";
import type { IntakeDetail, IntakeListItem, IntakeNoteItem } from "../types/intake";

type CreateIntakePayload = {
  prospectiveClientName: string;
  prospectiveClientEmail?: string;
  prospectiveClientPhone?: string;
  practiceArea: string;
  intakeType: string;
  ownerActorId: string;
  ownerDisplayName: string;
  nextAction?: string;
  nextActionDueAtUtc?: string;
  referralSource?: string;
  conflictCheckId?: string;
  notesSummary?: string;
};

type ChangeStagePayload = {
  toStage: string;
  changeReason?: string;
  nextAction?: string;
  nextActionDueAtUtc?: string;
  declineReason?: string;
  approvedConflictDecisionId?: string;
};

type ConvertToMatterPayload = {
  matterTitle: string;
  responsibleAttorneyActorId: string;
  responsibleAttorneyDisplayName?: string;
  paralegalActorId?: string;
  paralegalDisplayName?: string;
};

type ConvertToMatterResponse = {
  matterId: string;
  matterNumber: string;
};

export async function getIntakeList(token: string): Promise<IntakeListItem[]> {
  return getJson<IntakeListItem[]>("/api/intake", undefined, token);
}

export async function getIntakeById(token: string, intakeId: string): Promise<IntakeDetail> {
  return getJson<IntakeDetail>(`/api/intake/${intakeId}`, undefined, token);
}

export async function createIntake(token: string, payload: CreateIntakePayload): Promise<IntakeListItem> {
  return postJson<IntakeListItem, CreateIntakePayload>("/api/intake", payload, token);
}

export async function changeIntakeStage(token: string, intakeId: string, payload: ChangeStagePayload): Promise<void> {
  await postJson<{ id: string; stage: string }, ChangeStagePayload>(`/api/intake/${intakeId}/stage`, payload, token);
}

export async function addIntakeNote(token: string, intakeId: string, note: string): Promise<IntakeNoteItem> {
  return postJson<IntakeNoteItem, { note: string }>(`/api/intake/${intakeId}/notes`, { note }, token);
}

export async function convertIntakeToMatter(
  token: string,
  intakeId: string,
  payload: ConvertToMatterPayload,
): Promise<ConvertToMatterResponse> {
  return postJson<ConvertToMatterResponse, ConvertToMatterPayload>(
    `/api/intake/${intakeId}/convert-to-matter`,
    payload,
    token,
  );
}

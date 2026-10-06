import { postJson } from "./apiClient";
import type { ConflictCheckResult } from "../types/parties";

type ConflictCheckPayload = {
  query: string;
};

type ConflictDecisionPayload = {
  decision: "Approved" | "Rejected" | "NeedsMoreInfo";
  rationale: string;
};

type ConflictDecisionResult = {
  conflictDecisionId: string;
  conflictCheckId: string;
  decision: string;
  rationale: string;
  decidedBy: string;
  decidedAtUtc: string;
};

export async function runConflictCheck(token: string, query: string): Promise<ConflictCheckResult> {
  return postJson<ConflictCheckResult, ConflictCheckPayload>("/api/conflicts/checks", { query }, token);
}

export async function submitConflictDecision(
  token: string,
  checkId: string,
  payload: ConflictDecisionPayload,
): Promise<ConflictDecisionResult> {
  return postJson<ConflictDecisionResult, ConflictDecisionPayload>(`/api/conflicts/checks/${checkId}/decisions`, payload, token);
}

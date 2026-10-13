export type IntakeListItem = {
  id: string;
  prospectiveClientName: string;
  practiceArea: string;
  intakeType: string;
  stage: string;
  ownerActorId: string;
  ownerDisplayName: string;
  nextAction: string | null;
  nextActionDueAtUtc: string | null;
  referralSource: string | null;
  conflictCheckId: string | null;
  approvedConflictDecisionId: string | null;
  createdAtUtc: string;
};

export type IntakeStageHistoryItem = {
  id: string;
  fromStage: string;
  toStage: string;
  changedByActorId: string;
  changedByDisplayName: string;
  changeReason: string | null;
  changedAtUtc: string;
};

export type IntakeNoteItem = {
  id: string;
  note: string;
  createdByActorId: string;
  createdByDisplayName: string;
  createdAtUtc: string;
};

export type IntakeDetail = {
  id: string;
  prospectiveClientName: string;
  prospectiveClientEmail: string | null;
  prospectiveClientPhone: string | null;
  practiceArea: string;
  intakeType: string;
  stage: string;
  ownerActorId: string;
  ownerDisplayName: string;
  nextAction: string | null;
  nextActionDueAtUtc: string | null;
  referralSource: string | null;
  conflictCheckId: string | null;
  approvedConflictDecisionId: string | null;
  declineReason: string | null;
  notesSummary: string | null;
  createdAtUtc: string;
  stageHistory: IntakeStageHistoryItem[];
  notes: IntakeNoteItem[];
};

export type MatterListItem = {
  id: string;
  matterNumber: string;
  title: string;
  practiceArea: string;
  stage: string;
  status: string;
  responsibleAttorneyActorId: string;
  responsibleAttorneyDisplayName: string;
  paralegalActorId: string | null;
  paralegalDisplayName: string | null;
  linkedIntakeRecordId: string | null;
  createdAtUtc: string;
};

export type MatterStageHistoryItem = {
  id: string;
  fromStage: string;
  toStage: string;
  changedByActorId: string;
  changedByDisplayName: string;
  changeReason: string | null;
  changedAtUtc: string;
};

export type MatterDetail = {
  id: string;
  matterNumber: string;
  title: string;
  practiceArea: string;
  stage: string;
  status: string;
  responsibleAttorneyActorId: string;
  responsibleAttorneyDisplayName: string;
  paralegalActorId: string | null;
  paralegalDisplayName: string | null;
  linkedIntakeRecordId: string | null;
  createdAtUtc: string;
  stageHistory: MatterStageHistoryItem[];
};

export type MatterTask = {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priority: string;
  assigneeActorId: string | null;
  assigneeDisplayName: string | null;
  dueAtUtc: string | null;
  isDeadlineVerified: boolean;
  deadlineVerifiedAtUtc: string | null;
  deadlineVerifiedByActorId: string | null;
  deadlineVerifiedByDisplayName: string | null;
  completedAtUtc: string | null;
  createdByActorId: string;
  createdByDisplayName: string;
  createdAtUtc: string;
};

export type CreateMatterTaskRequest = {
  title: string;
  description?: string;
  status?: string;
  priority?: string;
  assigneeActorId?: string;
  assigneeDisplayName?: string;
  dueAtUtc?: string;
  isDeadlineVerified: boolean;
};

export type UpdateMatterTaskRequest = {
  title?: string;
  description?: string;
  status?: string;
  priority?: string;
  assigneeActorId?: string;
  assigneeDisplayName?: string;
  dueAtUtc?: string;
  isDeadlineVerified?: boolean;
};

export type MatterDocument = {
  id: string;
  fileName: string;
  documentType: string;
  storageKey: string | null;
  uploadedByActorId: string;
  uploadedByDisplayName: string;
  uploadedAtUtc: string;
  reviewStatus: string;
  reviewNotes: string | null;
  reviewedByActorId: string | null;
  reviewedByDisplayName: string | null;
  reviewedAtUtc: string | null;
};

export type CreateMatterDocumentRequest = {
  fileName: string;
  documentType: string;
  storageKey?: string;
};

export type ReviewMatterDocumentRequest = {
  reviewStatus: string;
  reviewNotes?: string;
};

export type MatterTimelineAction = {
  id: string;
  actionType: string;
  summary: string;
  details: string | null;
  occurredAtUtc: string;
  createdByActorId: string;
  createdByDisplayName: string;
  createdAtUtc: string;
};

export type CreateMatterTimelineActionRequest = {
  actionType: string;
  summary: string;
  details?: string;
  occurredAtUtc?: string;
};

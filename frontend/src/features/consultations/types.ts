export type ConsultationSlot = {
  startsAtUtc: string;
  endsAtUtc: string;
};

export type ConsultationRequestItem = {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  practiceArea: string;
  message: string | null;
  preferredAtUtc: string;
  timeZone: string;
  status: string;
  assignedToActorId: string | null;
  assignedToDisplayName: string | null;
  internalNotes: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

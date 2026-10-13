export type ConflictMatch = {
  partyId: string;
  displayName: string;
  partyType: string;
  isRestricted: boolean;
  score: number;
  matchReason: string;
};

export type ConflictCheckResult = {
  checkId: string;
  query: string;
  requestedAtUtc: string;
  matches: ConflictMatch[];
  correlationId: string;
};

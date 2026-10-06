export type PartyAlias = {
  id: string;
  alias: string;
  aliasType: string;
};

export type Party = {
  id: string;
  partyType: "Person" | "Organization";
  displayName: string;
  primaryEmail: string | null;
  primaryPhone: string | null;
  isRestricted: boolean;
  aliases: PartyAlias[];
};

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

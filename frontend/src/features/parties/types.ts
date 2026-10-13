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

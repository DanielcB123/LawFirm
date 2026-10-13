export type ActorProfile = {
  id: string;
  email: string;
  displayName: string;
  role: string;
  permissions: string[];
};

export type LoginResponse = {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: ActorProfile;
};

export type IdentityResponse = {
  actorId: string;
  email: string;
  displayName: string;
  role: string;
  permissions: string[];
  correlationId: string;
};

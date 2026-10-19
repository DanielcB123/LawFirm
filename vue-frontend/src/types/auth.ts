export type ActorIdentity = {
  id: number
  name: string
  email: string
  role: string
  permissions: string[]
}

export type LoginResponse = {
  access_token: string
  token_type: string
  user: ActorIdentity
}

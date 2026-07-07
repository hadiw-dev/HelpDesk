export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  department: string | null
  jobTitle: string | null
  roles: string[]
}

export interface AuthResult {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  user: UserProfile
}

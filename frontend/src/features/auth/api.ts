import { axiosInstance } from '@/api/axiosInstance'
import type { AuthResult, UserProfile } from '@/types/auth'

export interface RegisterInput {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
}

export interface LoginInput {
  email: string
  password: string
}

export interface ForgotPasswordInput {
  email: string
}

export interface ResetPasswordInput {
  email: string
  token: string
  newPassword: string
  confirmPassword: string
}

export interface ChangePasswordInput {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export interface UpdateProfileInput {
  firstName: string
  lastName: string
  department?: string
  jobTitle?: string
}

export const authApi = {
  register: (data: RegisterInput) =>
    axiosInstance.post<AuthResult>('/auth/register', data).then((res) => res.data),

  login: (data: LoginInput) =>
    axiosInstance.post<AuthResult>('/auth/login', data).then((res) => res.data),

  refreshToken: (refreshToken: string) =>
    axiosInstance.post<AuthResult>('/auth/refresh-token', { refreshToken }).then((res) => res.data),

  logout: (refreshToken: string) => axiosInstance.post('/auth/logout', { refreshToken }),

  forgotPassword: (data: ForgotPasswordInput) => axiosInstance.post('/auth/forgot-password', data),

  resetPassword: (data: ResetPasswordInput) => axiosInstance.post('/auth/reset-password', data),

  changePassword: (data: ChangePasswordInput) => axiosInstance.post('/auth/change-password', data),

  getProfile: () => axiosInstance.get<UserProfile>('/auth/profile').then((res) => res.data),

  updateProfile: (data: UpdateProfileInput) =>
    axiosInstance.put<UserProfile>('/auth/profile', data).then((res) => res.data),
}

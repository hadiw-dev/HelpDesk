import { axiosInstance } from '@/api/axiosInstance'
import type { AdminUser, AdminUserQueryParams } from '@/types/admin'
import type { PagedResult } from '@/types/tickets'

export interface CreateUserInput {
  email: string
  password: string
  firstName: string
  lastName: string
  department?: string
  jobTitle?: string
  role: string
}

export interface UpdateUserInput {
  firstName: string
  lastName: string
  department?: string
  jobTitle?: string
  isActive: boolean
}

export const adminUsersApi = {
  search: (params: AdminUserQueryParams) =>
    axiosInstance.get<PagedResult<AdminUser>>('/admin/users', { params }).then((res) => res.data),

  getById: (id: string) => axiosInstance.get<AdminUser>(`/admin/users/${id}`).then((res) => res.data),

  create: (data: CreateUserInput) => axiosInstance.post<AdminUser>('/admin/users', data).then((res) => res.data),

  update: (id: string, data: UpdateUserInput) =>
    axiosInstance.put<AdminUser>(`/admin/users/${id}`, data).then((res) => res.data),

  changeRole: (id: string, role: string) =>
    axiosInstance.put<AdminUser>(`/admin/users/${id}/role`, { role }).then((res) => res.data),

  remove: (id: string) => axiosInstance.delete(`/admin/users/${id}`),
}

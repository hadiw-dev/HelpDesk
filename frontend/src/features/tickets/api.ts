import { axiosInstance } from '@/api/axiosInstance'
import type { PagedResult, TicketDetail, TicketHistoryEntry, TicketListItem, TicketQueryParams } from '@/types/tickets'

export interface CreateTicketInput {
  title: string
  description: string
  categoryId: string
  priorityId: string
  dueDate?: string | null
}

export interface UpdateTicketInput {
  title: string
  description: string
  categoryId: string
  priorityId: string
  statusId: string
  dueDate?: string | null
}

export const ticketsApi = {
  search: (params: TicketQueryParams) =>
    axiosInstance.get<PagedResult<TicketListItem>>('/tickets', { params }).then((res) => res.data),

  getById: (id: string) => axiosInstance.get<TicketDetail>(`/tickets/${id}`).then((res) => res.data),

  getHistory: (id: string) =>
    axiosInstance.get<TicketHistoryEntry[]>(`/tickets/${id}/history`).then((res) => res.data),

  create: (data: CreateTicketInput) =>
    axiosInstance.post<TicketDetail>('/tickets', data).then((res) => res.data),

  update: (id: string, data: UpdateTicketInput) =>
    axiosInstance.put<TicketDetail>(`/tickets/${id}`, data).then((res) => res.data),

  remove: (id: string) => axiosInstance.delete(`/tickets/${id}`),

  restore: (id: string) => axiosInstance.post<TicketDetail>(`/tickets/${id}/restore`).then((res) => res.data),
}

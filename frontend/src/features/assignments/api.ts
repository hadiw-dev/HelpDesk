import { axiosInstance } from '@/api/axiosInstance'
import type { AssignmentHistoryEntry } from '@/types/assignments'
import type { TicketDetail } from '@/types/tickets'

export const assignmentsApi = {
  assign: (ticketId: string, assignedToUserId: string | null) =>
    axiosInstance.post<TicketDetail>(`/tickets/${ticketId}/assign`, { assignedToUserId }).then((res) => res.data),

  autoAssign: (ticketId: string) =>
    axiosInstance.post<TicketDetail>(`/tickets/${ticketId}/auto-assign`).then((res) => res.data),

  getHistory: (ticketId: string) =>
    axiosInstance.get<AssignmentHistoryEntry[]>(`/tickets/${ticketId}/assignments`).then((res) => res.data),
}

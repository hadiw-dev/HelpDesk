import { axiosInstance } from '@/api/axiosInstance'
import type { Comment } from '@/types/comments'

export interface CreateCommentInput {
  content: string
  isInternal: boolean
}

export const commentsApi = {
  getForTicket: (ticketId: string) =>
    axiosInstance.get<Comment[]>(`/tickets/${ticketId}/comments`).then((res) => res.data),

  add: (ticketId: string, data: CreateCommentInput) =>
    axiosInstance.post<Comment>(`/tickets/${ticketId}/comments`, data).then((res) => res.data),
}

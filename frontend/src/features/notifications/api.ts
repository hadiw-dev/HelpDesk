import { axiosInstance } from '@/api/axiosInstance'
import type { NotificationItem } from '@/types/notifications'
import type { PagedResult } from '@/types/tickets'

export interface NotificationQueryParams {
  page?: number
  pageSize?: number
  unreadOnly?: boolean
}

export const notificationsApi = {
  search: (params: NotificationQueryParams) =>
    axiosInstance.get<PagedResult<NotificationItem>>('/notifications', { params }).then((res) => res.data),

  getUnreadCount: () => axiosInstance.get<number>('/notifications/unread-count').then((res) => res.data),

  markAsRead: (id: string) => axiosInstance.post(`/notifications/${id}/read`),

  markAllAsRead: () => axiosInstance.post('/notifications/read-all'),
}

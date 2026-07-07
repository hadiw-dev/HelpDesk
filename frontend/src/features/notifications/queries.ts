import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { notificationsApi, type NotificationQueryParams } from '@/features/notifications/api'

const POLL_INTERVAL_MS = 30_000

export function useNotificationsQuery(params: NotificationQueryParams = {}) {
  return useQuery({
    queryKey: ['notifications', 'list', params],
    queryFn: () => notificationsApi.search(params),
    refetchInterval: POLL_INTERVAL_MS,
  })
}

export function useUnreadNotificationCountQuery() {
  return useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: notificationsApi.getUnreadCount,
    refetchInterval: POLL_INTERVAL_MS,
  })
}

export function useMarkNotificationReadMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['notifications'] })
    },
  })
}

export function useMarkAllNotificationsReadMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['notifications'] })
    },
  })
}

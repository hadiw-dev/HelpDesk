import { useQuery } from '@tanstack/react-query'
import { activityLogsApi } from '@/features/admin/activityLogs/api'
import type { ActivityLogQueryParams } from '@/types/admin'

export function useActivityLogsQuery(params: ActivityLogQueryParams) {
  return useQuery({
    queryKey: ['admin', 'activity-logs', params],
    queryFn: () => activityLogsApi.search(params),
    placeholderData: (previousData) => previousData,
  })
}

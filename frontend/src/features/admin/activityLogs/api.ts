import { axiosInstance } from '@/api/axiosInstance'
import type { ActivityLogEntry, ActivityLogQueryParams } from '@/types/admin'
import type { PagedResult } from '@/types/tickets'

export const activityLogsApi = {
  search: (params: ActivityLogQueryParams) =>
    axiosInstance.get<PagedResult<ActivityLogEntry>>('/admin/activity-logs', { params }).then((res) => res.data),
}

import { axiosInstance } from '@/api/axiosInstance'
import type { SystemSettings } from '@/types/admin'

export const systemSettingsApi = {
  get: () => axiosInstance.get<SystemSettings>('/admin/settings').then((res) => res.data),
  update: (data: SystemSettings) => axiosInstance.put<SystemSettings>('/admin/settings', data).then((res) => res.data),
}

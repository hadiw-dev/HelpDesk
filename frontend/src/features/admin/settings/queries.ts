import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { systemSettingsApi } from '@/features/admin/settings/api'
import type { SystemSettings } from '@/types/admin'

const settingsKey = ['admin', 'settings'] as const

export function useSystemSettingsQuery() {
  return useQuery({ queryKey: settingsKey, queryFn: systemSettingsApi.get })
}

export function useUpdateSystemSettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: SystemSettings) => systemSettingsApi.update(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: settingsKey })
    },
  })
}

import { axiosInstance } from '@/api/axiosInstance'
import type { AdminLookupItem, LookupUpsertInput } from '@/types/admin'

export type LookupResource = 'categories' | 'priorities' | 'statuses'

export function createAdminLookupApi(resource: LookupResource) {
  const basePath = `/admin/${resource}`

  return {
    getAll: () => axiosInstance.get<AdminLookupItem[]>(basePath).then((res) => res.data),
    create: (data: LookupUpsertInput) => axiosInstance.post<AdminLookupItem>(basePath, data).then((res) => res.data),
    update: (id: string, data: LookupUpsertInput) =>
      axiosInstance.put<AdminLookupItem>(`${basePath}/${id}`, data).then((res) => res.data),
    remove: (id: string) => axiosInstance.delete(`${basePath}/${id}`),
  }
}

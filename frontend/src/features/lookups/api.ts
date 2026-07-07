import { axiosInstance } from '@/api/axiosInstance'
import type { LookupItem } from '@/types/lookups'

export const lookupsApi = {
  getCategories: () => axiosInstance.get<LookupItem[]>('/lookups/categories').then((res) => res.data),
  getPriorities: () => axiosInstance.get<LookupItem[]>('/lookups/priorities').then((res) => res.data),
  getStatuses: () => axiosInstance.get<LookupItem[]>('/lookups/statuses').then((res) => res.data),
  getAgents: () => axiosInstance.get<LookupItem[]>('/lookups/agents').then((res) => res.data),
}

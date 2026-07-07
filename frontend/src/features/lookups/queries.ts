import { useQuery } from '@tanstack/react-query'
import { lookupsApi } from '@/features/lookups/api'

const staleTime = 5 * 60 * 1000

export function useCategoriesQuery() {
  return useQuery({ queryKey: ['lookups', 'categories'], queryFn: lookupsApi.getCategories, staleTime })
}

export function usePrioritiesQuery() {
  return useQuery({ queryKey: ['lookups', 'priorities'], queryFn: lookupsApi.getPriorities, staleTime })
}

export function useStatusesQuery() {
  return useQuery({ queryKey: ['lookups', 'statuses'], queryFn: lookupsApi.getStatuses, staleTime })
}

/** `enabled` should be `false` for Employees — the endpoint is Agent+-only and would otherwise 403. */
export function useAgentsQuery(enabled: boolean) {
  return useQuery({ queryKey: ['lookups', 'agents'], queryFn: lookupsApi.getAgents, staleTime, enabled })
}

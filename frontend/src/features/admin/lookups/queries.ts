import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createAdminLookupApi, type LookupResource } from '@/features/admin/lookups/api'
import type { LookupUpsertInput } from '@/types/admin'

/** One hook, reused for Categories/Priorities/Statuses management — they're identical in shape. */
export function useAdminLookupQueries(resource: LookupResource) {
  const api = createAdminLookupApi(resource)
  const queryClient = useQueryClient()
  const queryKey = ['admin', resource] as const

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey })
    // The read-only dropdown lookups (ticket create/edit forms) share the same resource name.
    void queryClient.invalidateQueries({ queryKey: ['lookups', resource] })
  }

  const listQuery = useQuery({ queryKey, queryFn: api.getAll })

  const createMutation = useMutation({
    mutationFn: (data: LookupUpsertInput) => api.create(data),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: LookupUpsertInput }) => api.update(id, data),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.remove(id),
    onSuccess: invalidate,
  })

  return { listQuery, createMutation, updateMutation, deleteMutation }
}

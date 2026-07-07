import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { assignmentsApi } from '@/features/assignments/api'

export function useAssignmentHistoryQuery(ticketId: string | undefined) {
  return useQuery({
    queryKey: ['tickets', 'assignments', ticketId ?? ''],
    queryFn: () => assignmentsApi.getHistory(ticketId!),
    enabled: Boolean(ticketId),
  })
}

export function useAssignTicketMutation(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (assignedToUserId: string | null) => assignmentsApi.assign(ticketId, assignedToUserId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

export function useAutoAssignTicketMutation(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => assignmentsApi.autoAssign(ticketId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

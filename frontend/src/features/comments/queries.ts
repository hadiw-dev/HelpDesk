import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { commentsApi, type CreateCommentInput } from '@/features/comments/api'

const commentsKey = (ticketId: string) => ['tickets', 'comments', ticketId] as const

export function useCommentsQuery(ticketId: string | undefined) {
  return useQuery({
    queryKey: commentsKey(ticketId ?? ''),
    queryFn: () => commentsApi.getForTicket(ticketId!),
    enabled: Boolean(ticketId),
  })
}

export function useAddCommentMutation(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateCommentInput) => commentsApi.add(ticketId, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: commentsKey(ticketId) })
    },
  })
}

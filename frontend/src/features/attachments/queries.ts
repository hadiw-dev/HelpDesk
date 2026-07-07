import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { attachmentsApi } from '@/features/attachments/api'

const attachmentsKey = (ticketId: string) => ['tickets', 'attachments', ticketId] as const

export function useAttachmentsQuery(ticketId: string | undefined) {
  return useQuery({
    queryKey: attachmentsKey(ticketId ?? ''),
    queryFn: () => attachmentsApi.getForTicket(ticketId!),
    enabled: Boolean(ticketId),
  })
}

export function useUploadAttachmentMutation(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (file: File) => attachmentsApi.upload(ticketId, file),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: attachmentsKey(ticketId) })
    },
  })
}

export function useDeleteAttachmentMutation(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (attachmentId: string) => attachmentsApi.remove(ticketId, attachmentId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: attachmentsKey(ticketId) })
    },
  })
}

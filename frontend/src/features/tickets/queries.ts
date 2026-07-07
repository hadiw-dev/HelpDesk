import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ticketsApi, type CreateTicketInput, type UpdateTicketInput } from '@/features/tickets/api'
import type { TicketQueryParams } from '@/types/tickets'

const ticketsKey = (params: TicketQueryParams) => ['tickets', params] as const
const ticketKey = (id: string) => ['tickets', 'detail', id] as const
const ticketHistoryKey = (id: string) => ['tickets', 'history', id] as const

export function useTicketsQuery(params: TicketQueryParams) {
  return useQuery({
    queryKey: ticketsKey(params),
    queryFn: () => ticketsApi.search(params),
    placeholderData: (previousData) => previousData,
  })
}

export function useTicketQuery(id: string | undefined) {
  return useQuery({
    queryKey: ticketKey(id ?? ''),
    queryFn: () => ticketsApi.getById(id!),
    enabled: Boolean(id),
  })
}

export function useTicketHistoryQuery(id: string | undefined) {
  return useQuery({
    queryKey: ticketHistoryKey(id ?? ''),
    queryFn: () => ticketsApi.getHistory(id!),
    enabled: Boolean(id),
  })
}

export function useCreateTicketMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateTicketInput) => ticketsApi.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

export function useUpdateTicketMutation(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: UpdateTicketInput) => ticketsApi.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

export function useDeleteTicketMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => ticketsApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

export function useRestoreTicketMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => ticketsApi.restore(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] })
    },
  })
}

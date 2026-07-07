import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { adminUsersApi, type CreateUserInput, type UpdateUserInput } from '@/features/admin/users/api'
import type { AdminUserQueryParams } from '@/types/admin'

const usersKey = ['admin', 'users'] as const

export function useAdminUsersQuery(params: AdminUserQueryParams) {
  return useQuery({
    queryKey: [...usersKey, params],
    queryFn: () => adminUsersApi.search(params),
    placeholderData: (previousData) => previousData,
  })
}

export function useCreateUserMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateUserInput) => adminUsersApi.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: usersKey })
    },
  })
}

export function useUpdateUserMutation(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: UpdateUserInput) => adminUsersApi.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: usersKey })
    },
  })
}

export function useChangeUserRoleMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, role }: { id: string; role: string }) => adminUsersApi.changeRole(id, role),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: usersKey })
    },
  })
}

export function useDeleteUserMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminUsersApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: usersKey })
    },
  })
}

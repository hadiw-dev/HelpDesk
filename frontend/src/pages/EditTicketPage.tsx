import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { Button, buttonVariants } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { useCategoriesQuery, usePrioritiesQuery, useStatusesQuery } from '@/features/lookups/queries'
import type { UpdateTicketInput } from '@/features/tickets/api'
import { useTicketQuery, useUpdateTicketMutation } from '@/features/tickets/queries'
import { updateTicketSchema, type UpdateTicketFormValues } from '@/features/tickets/schemas'
import type { TicketDetail } from '@/types/tickets'
import { extractErrorMessage } from '@/utils/errors'
import { isAgentOrAbove } from '@/utils/roles'

const inputClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50 disabled:opacity-60'

function toUpdatePayload(ticket: TicketDetail, overrides: Partial<UpdateTicketInput> = {}): UpdateTicketInput {
  return {
    title: ticket.title,
    description: ticket.description,
    categoryId: ticket.categoryId,
    priorityId: ticket.priorityId,
    statusId: ticket.statusId,
    dueDate: ticket.dueDate,
    ...overrides,
  }
}

export function EditTicketPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)

  const { data: ticket, isLoading, isError } = useTicketQuery(id)
  const { data: categories } = useCategoriesQuery()
  const { data: priorities } = usePrioritiesQuery()
  const { data: statuses } = useStatusesQuery()
  const updateTicket = useUpdateTicketMutation(id ?? '')

  const canEditFully = isAgentOrAbove(user?.roles ?? [])

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateTicketFormValues>({
    resolver: zodResolver(updateTicketSchema),
    values: ticket
      ? {
          title: ticket.title,
          description: ticket.description,
          categoryId: ticket.categoryId,
          priorityId: ticket.priorityId,
          statusId: ticket.statusId,
          dueDate: ticket.dueDate ? ticket.dueDate.slice(0, 10) : '',
        }
      : undefined,
  })

  const onSubmit = async (values: UpdateTicketFormValues) => {
    if (!ticket) return
    setServerError(null)
    try {
      await updateTicket.mutateAsync(
        toUpdatePayload(ticket, {
          title: values.title,
          description: values.description,
          categoryId: values.categoryId,
          priorityId: values.priorityId,
          statusId: values.statusId,
          dueDate: values.dueDate || null,
        }),
      )
      navigate(`/tickets/${id}`)
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">Loading ticket...</p>
  }

  if (isError || !ticket) {
    return <p className="text-sm text-destructive">Failed to load this ticket.</p>
  }

  return (
    <div className="max-w-xl space-y-4">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Edit {ticket.ticketNumber}</h1>
        {!canEditFully && (
          <p className="text-sm text-muted-foreground">
            You can update the title and description while this ticket is Open.
          </p>
        )}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {serverError && <p className="text-sm text-destructive">{serverError}</p>}

        <div className="space-y-1">
          <label htmlFor="title" className="text-sm font-medium">
            Title
          </label>
          <input id="title" className={inputClassName} {...register('title')} />
          {errors.title && <p className="text-xs text-destructive">{errors.title.message}</p>}
        </div>

        <div className="space-y-1">
          <label htmlFor="description" className="text-sm font-medium">
            Description
          </label>
          <textarea
            id="description"
            rows={5}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
            {...register('description')}
          />
          {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <label htmlFor="categoryId" className="text-sm font-medium">
              Category
            </label>
            <select id="categoryId" className={inputClassName} disabled={!canEditFully} {...register('categoryId')}>
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1">
            <label htmlFor="priorityId" className="text-sm font-medium">
              Priority
            </label>
            <select id="priorityId" className={inputClassName} disabled={!canEditFully} {...register('priorityId')}>
              {priorities?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <label htmlFor="statusId" className="text-sm font-medium">
              Status
            </label>
            <select id="statusId" className={inputClassName} disabled={!canEditFully} {...register('statusId')}>
              {statuses?.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1">
            <label htmlFor="dueDate" className="text-sm font-medium">
              Due date
            </label>
            <input id="dueDate" type="date" className={inputClassName} {...register('dueDate')} />
          </div>
        </div>

        {canEditFully && (
          <p className="text-xs text-muted-foreground">
            Assigned to: {ticket.assignedToName ?? 'Unassigned'} — use the Assignment panel on the ticket details page
            to reassign.
          </p>
        )}

        <div className="flex gap-2">
          <Button type="submit" disabled={updateTicket.isPending}>
            {updateTicket.isPending ? 'Saving...' : 'Save changes'}
          </Button>
          <Link to={`/tickets/${id}`} className={buttonVariants({ variant: 'outline' })}>
            Cancel
          </Link>
        </div>
      </form>
    </div>
  )
}

import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { Button, buttonVariants } from '@/components/ui/button'
import { useCategoriesQuery, usePrioritiesQuery } from '@/features/lookups/queries'
import { useCreateTicketMutation } from '@/features/tickets/queries'
import { createTicketSchema, type CreateTicketFormValues } from '@/features/tickets/schemas'
import { extractErrorMessage } from '@/utils/errors'

const inputClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

export function CreateTicketPage() {
  const navigate = useNavigate()
  const [serverError, setServerError] = useState<string | null>(null)

  const { data: categories } = useCategoriesQuery()
  const { data: priorities } = usePrioritiesQuery()
  const createTicket = useCreateTicketMutation()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateTicketFormValues>({ resolver: zodResolver(createTicketSchema) })

  const onSubmit = async (values: CreateTicketFormValues) => {
    setServerError(null)
    try {
      const ticket = await createTicket.mutateAsync({
        title: values.title,
        description: values.description,
        categoryId: values.categoryId,
        priorityId: values.priorityId,
        dueDate: values.dueDate || null,
      })
      navigate(`/tickets/${ticket.id}`, { replace: true })
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  return (
    <div className="max-w-xl space-y-4">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">New ticket</h1>
        <p className="text-sm text-muted-foreground">Describe the issue you&apos;re experiencing.</p>
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
            <select id="categoryId" className={inputClassName} defaultValue="" {...register('categoryId')}>
              <option value="" disabled>
                Select category
              </option>
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            {errors.categoryId && <p className="text-xs text-destructive">{errors.categoryId.message}</p>}
          </div>

          <div className="space-y-1">
            <label htmlFor="priorityId" className="text-sm font-medium">
              Priority
            </label>
            <select id="priorityId" className={inputClassName} defaultValue="" {...register('priorityId')}>
              <option value="" disabled>
                Select priority
              </option>
              {priorities?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
            </select>
            {errors.priorityId && <p className="text-xs text-destructive">{errors.priorityId.message}</p>}
          </div>
        </div>

        <div className="space-y-1">
          <label htmlFor="dueDate" className="text-sm font-medium">
            Due date (optional)
          </label>
          <input id="dueDate" type="date" className={inputClassName} {...register('dueDate')} />
        </div>

        <div className="flex gap-2">
          <Button type="submit" disabled={createTicket.isPending}>
            {createTicket.isPending ? 'Creating...' : 'Create ticket'}
          </Button>
          <Link to="/tickets" className={buttonVariants({ variant: 'outline' })}>
            Cancel
          </Link>
        </div>
      </form>
    </div>
  )
}

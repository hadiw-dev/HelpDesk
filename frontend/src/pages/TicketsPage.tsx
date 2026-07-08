import { useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { buttonVariants } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { useCategoriesQuery, usePrioritiesQuery, useStatusesQuery } from '@/features/lookups/queries'
import { useTicketsQuery } from '@/features/tickets/queries'
import { isAgentOrAbove } from '@/utils/roles'
import { cn } from '@/lib/utils'

const selectClassName =
  'h-9 rounded-md border border-input bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

export function TicketsPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()

  const searchTerm = searchParams.get('q') ?? ''
  const categoryId = searchParams.get('categoryId') ?? ''
  const priorityId = searchParams.get('priorityId') ?? ''
  const statusId = searchParams.get('statusId') ?? ''
  const sortBy = searchParams.get('sortBy') ?? 'createdAt'
  const sortDescending = searchParams.get('sortDescending') !== 'false'
  const page = Number(searchParams.get('page') ?? '1')
  const pageSize = 10

  const [searchInput, setSearchInput] = useState(searchTerm)

  const { data: categories } = useCategoriesQuery()
  const { data: priorities } = usePrioritiesQuery()
  const { data: statuses } = useStatusesQuery()

  const queryParams = useMemo(
    () => ({
      searchTerm: searchTerm || undefined,
      categoryId: categoryId || undefined,
      priorityId: priorityId || undefined,
      statusId: statusId || undefined,
      page,
      pageSize,
      sortBy,
      sortDescending,
    }),
    [searchTerm, categoryId, priorityId, statusId, page, sortBy, sortDescending],
  )

  const { data, isLoading, isError, isFetching } = useTicketsQuery(queryParams)

  const updateParams = (updates: Record<string, string | undefined>) => {
    const next = new URLSearchParams(searchParams)
    for (const [key, value] of Object.entries(updates)) {
      if (value) {
        next.set(key, value)
      } else {
        next.delete(key)
      }
    }
    if (!('page' in updates)) {
      next.delete('page')
    }
    setSearchParams(next)
  }

  const canSeeAllTickets = isAgentOrAbove(user?.roles ?? [])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Tickets</h1>
          <p className="text-sm text-muted-foreground">{canSeeAllTickets ? 'All tickets' : 'Your tickets'}</p>
        </div>
        <Link to="/tickets/new" className={buttonVariants({ variant: 'default' })}>
          New ticket
        </Link>
      </div>

      <form
        className="flex flex-wrap items-center gap-2"
        onSubmit={(e) => {
          e.preventDefault()
          updateParams({ q: searchInput || undefined })
        }}
      >
        <label htmlFor="ticket-search-input" className="sr-only">
          Search tickets by title or ticket number
        </label>
        <input
          id="ticket-search-input"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder="Search by title or ticket number..."
          className="h-9 w-64 rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
        />

        <label htmlFor="ticket-category-filter" className="sr-only">
          Filter by category
        </label>
        <select
          id="ticket-category-filter"
          value={categoryId}
          onChange={(e) => updateParams({ categoryId: e.target.value })}
          className={selectClassName}
        >
          <option value="">All categories</option>
          {categories?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>

        <label htmlFor="ticket-priority-filter" className="sr-only">
          Filter by priority
        </label>
        <select
          id="ticket-priority-filter"
          value={priorityId}
          onChange={(e) => updateParams({ priorityId: e.target.value })}
          className={selectClassName}
        >
          <option value="">All priorities</option>
          {priorities?.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>

        <label htmlFor="ticket-status-filter" className="sr-only">
          Filter by status
        </label>
        <select
          id="ticket-status-filter"
          value={statusId}
          onChange={(e) => updateParams({ statusId: e.target.value })}
          className={selectClassName}
        >
          <option value="">All statuses</option>
          {statuses?.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>

        <label htmlFor="ticket-sort-by" className="sr-only">
          Sort by
        </label>
        <select
          id="ticket-sort-by"
          value={sortBy}
          onChange={(e) => updateParams({ sortBy: e.target.value })}
          className={selectClassName}
        >
          <option value="createdAt">Created date</option>
          <option value="title">Title</option>
          <option value="priority">Priority</option>
          <option value="status">Status</option>
          <option value="dueDate">Due date</option>
          <option value="ticketnumber">Ticket number</option>
        </select>

        <button
          type="button"
          onClick={() => updateParams({ sortDescending: sortDescending ? 'false' : undefined })}
          className={cn(selectClassName, 'cursor-pointer')}
        >
          {sortDescending ? 'Descending' : 'Ascending'}
        </button>

        <button type="submit" className={buttonVariants({ variant: 'secondary', size: 'sm' })}>
          Search
        </button>
      </form>

      {isLoading && <p className="text-sm text-muted-foreground">Loading tickets...</p>}
      {isError && <p className="text-sm text-destructive">Failed to load tickets. Please try again.</p>}

      {data && (
        <>
          <div className="overflow-x-auto rounded-xl border border-border">
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/40 text-left text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-3 py-2">Ticket #</th>
                  <th className="px-3 py-2">Title</th>
                  <th className="px-3 py-2">Category</th>
                  <th className="px-3 py-2">Priority</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Assigned To</th>
                  <th className="px-3 py-2">Created</th>
                </tr>
              </thead>
              <tbody>
                {data.items.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-3 py-6 text-center text-muted-foreground">
                      No tickets found.
                    </td>
                  </tr>
                )}
                {data.items.map((ticket) => (
                  <tr key={ticket.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                    <td className="px-3 py-2">
                      <Link to={`/tickets/${ticket.id}`} className="font-medium text-primary hover:underline">
                        {ticket.ticketNumber}
                      </Link>
                    </td>
                    <td className="px-3 py-2">{ticket.title}</td>
                    <td className="px-3 py-2">{ticket.categoryName}</td>
                    <td className="px-3 py-2">{ticket.priorityName}</td>
                    <td className="px-3 py-2">{ticket.statusName}</td>
                    <td className="px-3 py-2">{ticket.assignedToName ?? '—'}</td>
                    <td className="px-3 py-2">{new Date(ticket.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {data.totalCount} ticket{data.totalCount === 1 ? '' : 's'} · page {data.page} of {Math.max(data.totalPages, 1)}
              {isFetching && ' · refreshing...'}
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => updateParams({ page: String(page - 1) })}
                className={cn(buttonVariants({ variant: 'outline', size: 'sm' }), 'disabled:opacity-50')}
              >
                Previous
              </button>
              <button
                type="button"
                disabled={page >= data.totalPages}
                onClick={() => updateParams({ page: String(page + 1) })}
                className={cn(buttonVariants({ variant: 'outline', size: 'sm' }), 'disabled:opacity-50')}
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}

      {!canSeeAllTickets && (
        <p className="text-xs text-muted-foreground">
          As an Employee, you can only see and edit tickets you created (while they&apos;re still Open).
        </p>
      )}
    </div>
  )
}

import { useMemo, useState } from 'react'
import { useActivityLogsQuery } from '@/features/admin/activityLogs/queries'
import { buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'

const inputClassName =
  'h-9 rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

export function ActivityLogPage() {
  const [actionFilter, setActionFilter] = useState('')
  const [page, setPage] = useState(1)

  const params = useMemo(() => ({ action: actionFilter || undefined, page, pageSize: 20 }), [actionFilter, page])
  const { data, isLoading, isError, isFetching } = useActivityLogsQuery(params)

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Activity Log</h1>
        <p className="text-sm text-muted-foreground">System-wide audit trail of significant actions.</p>
      </div>

      <input
        value={actionFilter}
        onChange={(e) => {
          setActionFilter(e.target.value)
          setPage(1)
        }}
        placeholder="Filter by action (e.g. UserCreated, TicketDeleted)..."
        className={cn(inputClassName, 'w-72')}
      />

      {isLoading && <p className="text-sm text-muted-foreground">Loading...</p>}
      {isError && <p className="text-sm text-destructive">Failed to load activity log.</p>}

      {data && (
        <>
          <div className="overflow-x-auto rounded-xl border border-border">
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/40 text-left text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-3 py-2">When</th>
                  <th className="px-3 py-2">User</th>
                  <th className="px-3 py-2">Action</th>
                  <th className="px-3 py-2">Details</th>
                  <th className="px-3 py-2">IP</th>
                </tr>
              </thead>
              <tbody>
                {data.items.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-3 py-6 text-center text-muted-foreground">
                      No activity recorded.
                    </td>
                  </tr>
                ) : (
                  data.items.map((entry) => (
                    <tr key={entry.id} className="border-b border-border last:border-0">
                      <td className="px-3 py-2 whitespace-nowrap">{new Date(entry.createdAt).toLocaleString()}</td>
                      <td className="px-3 py-2">{entry.userName}</td>
                      <td className="px-3 py-2 font-medium">{entry.action}</td>
                      <td className="px-3 py-2 text-muted-foreground">{entry.details ?? '—'}</td>
                      <td className="px-3 py-2 text-muted-foreground">{entry.ipAddress ?? '—'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {data.totalCount} entries · page {data.page} of {Math.max(data.totalPages, 1)}
              {isFetching && ' · refreshing...'}
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
                className={cn(buttonVariants({ variant: 'outline', size: 'sm' }), 'disabled:opacity-50')}
              >
                Previous
              </button>
              <button
                type="button"
                disabled={page >= data.totalPages}
                onClick={() => setPage((p) => p + 1)}
                className={cn(buttonVariants({ variant: 'outline', size: 'sm' }), 'disabled:opacity-50')}
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

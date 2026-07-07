import { Link } from 'react-router-dom'
import type { SlaReport } from '@/types/dashboard'

export function SlaDashboard({ data }: { data: SlaReport }) {
  const complianceAccent =
    data.compliancePercentage >= 90 ? 'text-emerald-600 dark:text-emerald-500' : data.compliancePercentage >= 70 ? 'text-amber-600 dark:text-amber-500' : 'text-destructive'

  return (
    <section className="space-y-4 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">SLA Dashboard</h2>

      {data.totalTrackedTickets === 0 ? (
        <p className="text-sm text-muted-foreground">
          No tickets have a due date set yet, so there&apos;s nothing to measure SLA compliance against.
        </p>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div>
              <p className="text-xs text-muted-foreground">Tracked</p>
              <p className="text-xl font-semibold">{data.totalTrackedTickets}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Met</p>
              <p className="text-xl font-semibold text-emerald-600 dark:text-emerald-500">{data.metCount}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Breached</p>
              <p className="text-xl font-semibold text-destructive">{data.breachedCount}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Compliance</p>
              <p className={`text-xl font-semibold ${complianceAccent}`}>{data.compliancePercentage}%</p>
            </div>
          </div>

          {data.breachedTickets.length > 0 && (
            <div className="overflow-x-auto rounded-lg border border-border">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/40 text-left text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-3 py-2">Ticket #</th>
                    <th className="px-3 py-2">Title</th>
                    <th className="px-3 py-2">Status</th>
                    <th className="px-3 py-2">Hours Overdue</th>
                  </tr>
                </thead>
                <tbody>
                  {data.breachedTickets.map((ticket) => (
                    <tr key={ticket.ticketId} className="border-b border-border last:border-0">
                      <td className="px-3 py-2">
                        <Link to={`/tickets/${ticket.ticketId}`} className="font-medium text-primary hover:underline">
                          {ticket.ticketNumber}
                        </Link>
                      </td>
                      <td className="px-3 py-2">{ticket.title}</td>
                      <td className="px-3 py-2">{ticket.statusName}</td>
                      <td className="px-3 py-2 text-destructive">{ticket.hoursOverdue.toFixed(1)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </section>
  )
}

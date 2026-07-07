import { useMemo, useState } from 'react'
import { BreakdownPieChart } from '@/components/dashboard/BreakdownPieChart'
import { DateRangeFilter } from '@/components/dashboard/DateRangeFilter'
import { KpiCard } from '@/components/dashboard/KpiCard'
import { MonthlyTicketsLineChart } from '@/components/dashboard/MonthlyTicketsLineChart'
import { ResolutionTimeBarChart } from '@/components/dashboard/ResolutionTimeBarChart'
import { SlaDashboard } from '@/components/dashboard/SlaDashboard'
import {
  useCategoryBreakdownQuery,
  useKpiSummaryQuery,
  useMonthlyTicketsQuery,
  usePriorityBreakdownQuery,
  useResolutionTimeQuery,
  useSlaReportQuery,
} from '@/features/dashboard/queries'

export function DashboardPage() {
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const params = useMemo(
    () => ({ dateFrom: dateFrom || undefined, dateTo: dateTo || undefined }),
    [dateFrom, dateTo],
  )

  const { data: kpi, isLoading: kpiLoading, isError: kpiError } = useKpiSummaryQuery(params)
  const { data: categories } = useCategoryBreakdownQuery(params)
  const { data: priorities } = usePriorityBreakdownQuery(params)
  const { data: monthly } = useMonthlyTicketsQuery(params)
  const { data: resolutionTime } = useResolutionTimeQuery(params)
  const { data: sla } = useSlaReportQuery(params)

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Dashboard</h1>
          <p className="text-sm text-muted-foreground">Ticket metrics and SLA compliance.</p>
        </div>
        <DateRangeFilter
          dateFrom={dateFrom}
          dateTo={dateTo}
          onChange={(next) => {
            setDateFrom(next.dateFrom)
            setDateTo(next.dateTo)
          }}
        />
      </div>

      {kpiLoading && <p className="text-sm text-muted-foreground">Loading dashboard...</p>}
      {kpiError && <p className="text-sm text-destructive">Failed to load dashboard data.</p>}

      {kpi && (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">
          <KpiCard label="Total Tickets" value={kpi.totalTickets} />
          <KpiCard label="Open" value={kpi.openTickets} />
          <KpiCard label="In Progress" value={kpi.inProgressTickets} />
          <KpiCard label="Pending" value={kpi.pendingTickets} />
          <KpiCard label="Resolved" value={kpi.resolvedTickets} />
          <KpiCard label="Closed" value={kpi.closedTickets} />
          <KpiCard label="Overdue" value={kpi.overdueTickets} accent={kpi.overdueTickets > 0 ? 'danger' : 'default'} />
          <KpiCard label="Unassigned" value={kpi.unassignedTickets} accent={kpi.unassignedTickets > 0 ? 'warning' : 'default'} />
          <KpiCard
            label="Avg. Resolution"
            value={kpi.averageResolutionHours !== null ? `${kpi.averageResolutionHours.toFixed(1)}h` : '—'}
          />
        </div>
      )}

      <div className="grid gap-4 sm:grid-cols-2">
        <BreakdownPieChart
          title="Tickets by Category"
          data={(categories ?? []).map((c) => ({ name: c.categoryName, count: c.count }))}
        />
        <BreakdownPieChart
          title="Tickets by Priority"
          data={(priorities ?? []).map((p) => ({ name: p.priorityName, count: p.count }))}
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <MonthlyTicketsLineChart data={monthly ?? []} />
        {resolutionTime && <ResolutionTimeBarChart data={resolutionTime} />}
      </div>

      {sla && <SlaDashboard data={sla} />}
    </div>
  )
}

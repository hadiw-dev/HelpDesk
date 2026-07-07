export interface KpiSummary {
  totalTickets: number
  openTickets: number
  inProgressTickets: number
  pendingTickets: number
  resolvedTickets: number
  closedTickets: number
  overdueTickets: number
  unassignedTickets: number
  averageResolutionHours: number | null
}

export interface CategoryBreakdownItem {
  categoryId: string
  categoryName: string
  count: number
}

export interface PriorityBreakdownItem {
  priorityId: string
  priorityName: string
  count: number
}

export interface MonthlyTicketsItem {
  year: number
  month: number
  monthLabel: string
  createdCount: number
  resolvedCount: number
}

export interface PriorityResolutionTime {
  priorityName: string
  averageResolutionHours: number | null
  ticketCount: number
}

export interface ResolutionTimeReport {
  overallAverageResolutionHours: number | null
  byPriority: PriorityResolutionTime[]
}

export interface SlaBreachEntry {
  ticketId: string
  ticketNumber: string
  title: string
  dueDate: string
  resolvedAt: string | null
  statusName: string
  hoursOverdue: number
}

export interface SlaReport {
  totalTrackedTickets: number
  metCount: number
  breachedCount: number
  compliancePercentage: number
  breachedTickets: SlaBreachEntry[]
}

export interface DashboardQueryParams {
  dateFrom?: string
  dateTo?: string
}

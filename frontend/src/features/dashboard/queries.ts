import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '@/features/dashboard/api'
import type { DashboardQueryParams } from '@/types/dashboard'

export function useKpiSummaryQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'kpi-summary', params],
    queryFn: () => dashboardApi.getKpiSummary(params),
  })
}

export function useCategoryBreakdownQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'category-breakdown', params],
    queryFn: () => dashboardApi.getCategoryBreakdown(params),
  })
}

export function usePriorityBreakdownQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'priority-breakdown', params],
    queryFn: () => dashboardApi.getPriorityBreakdown(params),
  })
}

export function useMonthlyTicketsQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'monthly-tickets', params],
    queryFn: () => dashboardApi.getMonthlyTickets(params),
  })
}

export function useResolutionTimeQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'resolution-time', params],
    queryFn: () => dashboardApi.getResolutionTime(params),
  })
}

export function useSlaReportQuery(params: DashboardQueryParams) {
  return useQuery({
    queryKey: ['dashboard', 'sla-report', params],
    queryFn: () => dashboardApi.getSlaReport(params),
  })
}

import { axiosInstance } from '@/api/axiosInstance'
import type {
  CategoryBreakdownItem,
  DashboardQueryParams,
  KpiSummary,
  MonthlyTicketsItem,
  PriorityBreakdownItem,
  ResolutionTimeReport,
  SlaReport,
} from '@/types/dashboard'

export const dashboardApi = {
  getKpiSummary: (params: DashboardQueryParams) =>
    axiosInstance.get<KpiSummary>('/dashboard/kpi-summary', { params }).then((res) => res.data),

  getCategoryBreakdown: (params: DashboardQueryParams) =>
    axiosInstance.get<CategoryBreakdownItem[]>('/dashboard/tickets-by-category', { params }).then((res) => res.data),

  getPriorityBreakdown: (params: DashboardQueryParams) =>
    axiosInstance.get<PriorityBreakdownItem[]>('/dashboard/tickets-by-priority', { params }).then((res) => res.data),

  getMonthlyTickets: (params: DashboardQueryParams) =>
    axiosInstance.get<MonthlyTicketsItem[]>('/dashboard/monthly-tickets', { params }).then((res) => res.data),

  getResolutionTime: (params: DashboardQueryParams) =>
    axiosInstance.get<ResolutionTimeReport>('/dashboard/resolution-time', { params }).then((res) => res.data),

  getSlaReport: (params: DashboardQueryParams) =>
    axiosInstance.get<SlaReport>('/dashboard/sla-report', { params }).then((res) => res.data),

  getPdfReport: (params: DashboardQueryParams) =>
    axiosInstance.get('/dashboard/reports/pdf', { params, responseType: 'blob' }).then((res) => res.data as Blob),

  getExcelReport: (params: DashboardQueryParams) =>
    axiosInstance.get('/dashboard/reports/excel', { params, responseType: 'blob' }).then((res) => res.data as Blob),
}

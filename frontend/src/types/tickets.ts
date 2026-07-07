export interface TicketListItem {
  id: string
  ticketNumber: string
  title: string
  categoryName: string
  priorityName: string
  statusName: string
  createdByName: string
  assignedToName: string | null
  dueDate: string | null
  createdAt: string
}

export interface TicketDetail {
  id: string
  ticketNumber: string
  title: string
  description: string
  categoryId: string
  categoryName: string
  priorityId: string
  priorityName: string
  statusId: string
  statusName: string
  createdByUserId: string
  createdByName: string
  assignedToUserId: string | null
  assignedToName: string | null
  dueDate: string | null
  resolvedAt: string | null
  closedAt: string | null
  createdAt: string
  updatedAt: string | null
}

export interface TicketHistoryEntry {
  id: string
  fieldName: string
  oldValue: string | null
  newValue: string | null
  changedByName: string | null
  changedAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface TicketQueryParams {
  searchTerm?: string
  categoryId?: string
  priorityId?: string
  statusId?: string
  assignedToUserId?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

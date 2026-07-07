export interface AdminUser {
  id: string
  email: string
  firstName: string
  lastName: string
  department: string | null
  jobTitle: string | null
  isActive: boolean
  roles: string[]
  createdAt: string
}

export interface AdminUserQueryParams {
  searchTerm?: string
  role?: string
  isActive?: boolean
  page?: number
  pageSize?: number
}

export interface AdminLookupItem {
  id: string
  name: string
  description: string | null
  displayOrder: number
  isActive: boolean
}

export interface LookupUpsertInput {
  name: string
  description?: string
  displayOrder: number
  isActive: boolean
}

export interface ActivityLogEntry {
  id: string
  userId: string | null
  userName: string
  action: string
  entityName: string
  entityId: string | null
  details: string | null
  ipAddress: string | null
  createdAt: string
}

export interface ActivityLogQueryParams {
  userId?: string
  action?: string
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}

export interface SystemSettings {
  siteName: string
  maxFileUploadSizeMb: number
  allowedFileExtensions: string
  defaultPageSize: number
}

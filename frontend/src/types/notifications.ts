export interface NotificationItem {
  id: string
  title: string
  message: string
  type: string
  isRead: boolean
  readAt: string | null
  relatedTicketId: string | null
  relatedTicketNumber: string | null
  createdAt: string
}

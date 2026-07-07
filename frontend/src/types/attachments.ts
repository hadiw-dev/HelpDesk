export interface TicketAttachment {
  id: string
  ticketId: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  uploadedByUserId: string
  uploadedByName: string
  createdAt: string
}

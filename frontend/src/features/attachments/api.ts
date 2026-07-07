import { axiosInstance } from '@/api/axiosInstance'
import type { TicketAttachment } from '@/types/attachments'

export const attachmentsApi = {
  getForTicket: (ticketId: string) =>
    axiosInstance.get<TicketAttachment[]>(`/tickets/${ticketId}/attachments`).then((res) => res.data),

  upload: (ticketId: string, file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return axiosInstance
      .post<TicketAttachment>(`/tickets/${ticketId}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((res) => res.data)
  },

  download: (ticketId: string, attachmentId: string) =>
    axiosInstance
      .get(`/tickets/${ticketId}/attachments/${attachmentId}/download`, { responseType: 'blob' })
      .then((res) => res.data as Blob),

  remove: (ticketId: string, attachmentId: string) =>
    axiosInstance.delete(`/tickets/${ticketId}/attachments/${attachmentId}`),
}

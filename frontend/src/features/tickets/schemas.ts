import { z } from 'zod'

export const createTicketSchema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  description: z.string().min(1, 'Description is required.').max(4000),
  categoryId: z.string().min(1, 'Category is required.'),
  priorityId: z.string().min(1, 'Priority is required.'),
  dueDate: z.string().optional().or(z.literal('')),
})
export type CreateTicketFormValues = z.infer<typeof createTicketSchema>

export const updateTicketSchema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  description: z.string().min(1, 'Description is required.').max(4000),
  categoryId: z.string().min(1, 'Category is required.'),
  priorityId: z.string().min(1, 'Priority is required.'),
  statusId: z.string().min(1, 'Status is required.'),
  dueDate: z.string().optional().or(z.literal('')),
})
export type UpdateTicketFormValues = z.infer<typeof updateTicketSchema>

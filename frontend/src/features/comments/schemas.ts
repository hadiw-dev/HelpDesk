import { z } from 'zod'

export const createCommentSchema = z.object({
  content: z.string().min(1, 'Comment cannot be empty.').max(4000),
})
export type CreateCommentFormValues = z.infer<typeof createCommentSchema>

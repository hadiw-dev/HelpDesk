export interface Comment {
  id: string
  ticketId: string
  content: string
  isInternal: boolean
  authorId: string
  authorName: string
  mentionedUserIds: string[]
  createdAt: string
}

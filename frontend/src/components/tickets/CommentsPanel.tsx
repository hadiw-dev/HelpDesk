import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { MentionText } from '@/components/tickets/MentionText'
import { MentionTextarea } from '@/components/tickets/MentionTextarea'
import { useAddCommentMutation } from '@/features/comments/queries'
import { createCommentSchema } from '@/features/comments/schemas'
import type { Comment } from '@/types/comments'
import type { MentionCandidate } from '@/utils/mentions'
import { extractErrorMessage } from '@/utils/errors'

interface CommentsPanelProps {
  ticketId: string
  comments: Comment[]
  candidates: MentionCandidate[]
  title: string
  isInternal: boolean
  emptyLabel: string
  placeholder: string
}

export function CommentsPanel({ ticketId, comments, candidates, title, isInternal, emptyLabel, placeholder }: CommentsPanelProps) {
  const [content, setContent] = useState('')
  const [error, setError] = useState<string | null>(null)
  const addComment = useAddCommentMutation(ticketId)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()

    const parsed = createCommentSchema.safeParse({ content })
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? 'Invalid comment.')
      return
    }

    setError(null)
    try {
      await addComment.mutateAsync({ content, isInternal })
      setContent('')
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <section className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">{title}</h2>

      {comments.length === 0 ? (
        <p className="text-sm text-muted-foreground">{emptyLabel}</p>
      ) : (
        <ul className="space-y-3">
          {comments.map((comment) => (
            <li key={comment.id} className="rounded-md bg-muted/30 p-2 text-sm">
              <p>
                <MentionText content={comment.content} />
              </p>
              <p className="mt-1 text-xs text-muted-foreground">
                {comment.authorName} · {new Date(comment.createdAt).toLocaleString()}
              </p>
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={(e) => void handleSubmit(e)} className="space-y-2">
        {error && <p className="text-xs text-destructive">{error}</p>}
        <MentionTextarea value={content} onChange={setContent} candidates={candidates} placeholder={placeholder} rows={3} />
        <Button type="submit" size="sm" disabled={addComment.isPending}>
          {addComment.isPending ? 'Posting...' : 'Post'}
        </Button>
      </form>
    </section>
  )
}

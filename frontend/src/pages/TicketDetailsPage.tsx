import { useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { Button, buttonVariants } from '@/components/ui/button'
import { AssignmentPanel } from '@/components/tickets/AssignmentPanel'
import { AttachmentsPanel } from '@/components/tickets/AttachmentsPanel'
import { CommentsPanel } from '@/components/tickets/CommentsPanel'
import { TicketTimeline } from '@/components/tickets/TicketTimeline'
import { useAuth } from '@/hooks/useAuth'
import { useAssignmentHistoryQuery } from '@/features/assignments/queries'
import { useCommentsQuery } from '@/features/comments/queries'
import { useAgentsQuery } from '@/features/lookups/queries'
import {
  useDeleteTicketMutation,
  useRestoreTicketMutation,
  useTicketHistoryQuery,
  useTicketQuery,
} from '@/features/tickets/queries'
import type { MentionCandidate } from '@/utils/mentions'
import { buildTimeline } from '@/utils/timeline'
import { extractErrorMessage } from '@/utils/errors'
import { isAgentOrAbove, isManagerOrAdmin } from '@/utils/roles'

function formatDate(value: string | null) {
  return value ? new Date(value).toLocaleString() : '—'
}

export function TicketDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: ticket, isLoading, isError } = useTicketQuery(id)
  const { data: history } = useTicketHistoryQuery(id)
  const { data: assignments } = useAssignmentHistoryQuery(id)
  const { data: comments } = useCommentsQuery(id)
  const deleteTicket = useDeleteTicketMutation()
  const restoreTicket = useRestoreTicketMutation()

  const canDelete = isAgentOrAbove(user?.roles ?? [])
  const canRestore = isManagerOrAdmin(user?.roles ?? [])
  const canEdit = ticket ? canDelete || (ticket.createdByUserId === user?.id && ticket.statusName === 'Open') : false

  const { data: agents } = useAgentsQuery(canDelete)

  const mentionCandidates = useMemo<MentionCandidate[]>(() => {
    const candidates: MentionCandidate[] = []
    if (ticket) {
      candidates.push({ id: ticket.createdByUserId, name: ticket.createdByName })
      if (ticket.assignedToUserId && ticket.assignedToName) {
        candidates.push({ id: ticket.assignedToUserId, name: ticket.assignedToName })
      }
    }
    for (const agent of agents ?? []) {
      if (!candidates.some((c) => c.id === agent.id)) {
        candidates.push(agent)
      }
    }
    return candidates
  }, [ticket, agents])

  const publicComments = useMemo(() => comments?.filter((c) => !c.isInternal) ?? [], [comments])
  const internalNotes = useMemo(() => comments?.filter((c) => c.isInternal) ?? [], [comments])

  const timeline = useMemo(
    () => buildTimeline(history ?? [], assignments ?? [], comments ?? []),
    [history, assignments, comments],
  )

  const handleDelete = async () => {
    if (!id || !window.confirm('Delete this ticket? It can be restored later by a Manager or Admin.')) {
      return
    }
    setActionError(null)
    try {
      await deleteTicket.mutateAsync(id)
      navigate('/tickets', { replace: true })
    } catch (error) {
      setActionError(extractErrorMessage(error))
    }
  }

  const handleRestore = async () => {
    if (!id) return
    setActionError(null)
    try {
      await restoreTicket.mutateAsync(id)
    } catch (error) {
      setActionError(extractErrorMessage(error))
    }
  }

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">Loading ticket...</p>
  }

  if (isError || !ticket) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">This ticket doesn&apos;t exist or you don&apos;t have access to it.</p>
        <Link to="/tickets" className="text-sm text-primary underline-offset-4 hover:underline">
          Back to tickets
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-muted-foreground">{ticket.ticketNumber}</p>
          <h1 className="text-xl font-semibold tracking-tight">{ticket.title}</h1>
        </div>
        <div className="flex gap-2">
          {canEdit && (
            <Link to={`/tickets/${ticket.id}/edit`} className={buttonVariants({ variant: 'outline' })}>
              Edit
            </Link>
          )}
          {canDelete && (
            <Button variant="destructive" onClick={() => void handleDelete()} disabled={deleteTicket.isPending}>
              Delete
            </Button>
          )}
          {canRestore && (
            <Button variant="outline" onClick={() => void handleRestore()} disabled={restoreTicket.isPending}>
              Restore
            </Button>
          )}
        </div>
      </div>

      {actionError && <p className="text-sm text-destructive">{actionError}</p>}

      <div className="grid gap-4 sm:grid-cols-2">
        <section className="space-y-3 rounded-xl border border-border p-4">
          <h2 className="text-sm font-semibold">Details</h2>
          <p className="whitespace-pre-wrap text-sm text-muted-foreground">{ticket.description}</p>
          <dl className="grid grid-cols-2 gap-y-2 text-sm">
            <dt className="text-muted-foreground">Category</dt>
            <dd>{ticket.categoryName}</dd>
            <dt className="text-muted-foreground">Priority</dt>
            <dd>{ticket.priorityName}</dd>
            <dt className="text-muted-foreground">Status</dt>
            <dd>{ticket.statusName}</dd>
            <dt className="text-muted-foreground">Created by</dt>
            <dd>{ticket.createdByName}</dd>
            <dt className="text-muted-foreground">Assigned to</dt>
            <dd>{ticket.assignedToName ?? 'Unassigned'}</dd>
            <dt className="text-muted-foreground">Due date</dt>
            <dd>{ticket.dueDate ? new Date(ticket.dueDate).toLocaleDateString() : '—'}</dd>
            <dt className="text-muted-foreground">Created</dt>
            <dd>{formatDate(ticket.createdAt)}</dd>
            <dt className="text-muted-foreground">Resolved</dt>
            <dd>{formatDate(ticket.resolvedAt)}</dd>
            <dt className="text-muted-foreground">Closed</dt>
            <dd>{formatDate(ticket.closedAt)}</dd>
          </dl>
        </section>

        <section className="space-y-3 rounded-xl border border-border p-4">
          <h2 className="text-sm font-semibold">Timeline</h2>
          <TicketTimeline entries={timeline} />
        </section>
      </div>

      {canDelete && <AssignmentPanel ticket={ticket} />}

      <AttachmentsPanel ticketId={ticket.id} />

      <div className="grid gap-4 sm:grid-cols-2">
        <CommentsPanel
          ticketId={ticket.id}
          comments={publicComments}
          candidates={mentionCandidates}
          title="Comments"
          isInternal={false}
          emptyLabel="No comments yet."
          placeholder="Add a comment... use @ to mention someone"
        />

        {canDelete && (
          <CommentsPanel
            ticketId={ticket.id}
            comments={internalNotes}
            candidates={mentionCandidates}
            title="Internal Notes"
            isInternal
            emptyLabel="No internal notes yet. Only Agents, Managers, and Admins can see these."
            placeholder="Add an internal note... use @ to mention someone"
          />
        )}
      </div>
    </div>
  )
}

import type { AssignmentHistoryEntry } from '@/types/assignments'
import type { Comment } from '@/types/comments'
import type { TicketHistoryEntry } from '@/types/tickets'

export type TimelineEntry =
  | { type: 'history'; id: string; timestamp: string; fieldName: string; oldValue: string | null; newValue: string | null; actorName: string | null }
  | {
      type: 'assignment'
      id: string
      timestamp: string
      previousAssignedToName: string | null
      assignedToName: string | null
      assignedByName: string | null
      assignmentType: string
    }
  | { type: 'comment'; id: string; timestamp: string; content: string; isInternal: boolean; authorName: string }

/**
 * Merges the three separate feeds into one chronological view. "AssignedTo" rows from the plain
 * ticket-history feed are dropped since the dedicated assignment feed already covers the same
 * events with richer detail (who assigned it, manual vs round robin).
 */
export function buildTimeline(
  history: TicketHistoryEntry[],
  assignments: AssignmentHistoryEntry[],
  comments: Comment[],
): TimelineEntry[] {
  const historyEntries: TimelineEntry[] = history
    .filter((h) => h.fieldName !== 'AssignedTo')
    .map((h) => ({
      type: 'history',
      id: h.id,
      timestamp: h.changedAt,
      fieldName: h.fieldName,
      oldValue: h.oldValue,
      newValue: h.newValue,
      actorName: h.changedByName,
    }))

  const assignmentEntries: TimelineEntry[] = assignments.map((a) => ({
    type: 'assignment',
    id: a.id,
    timestamp: a.assignedAt,
    previousAssignedToName: a.previousAssignedToName,
    assignedToName: a.assignedToName,
    assignedByName: a.assignedByName,
    assignmentType: a.assignmentType,
  }))

  const commentEntries: TimelineEntry[] = comments.map((c) => ({
    type: 'comment',
    id: c.id,
    timestamp: c.createdAt,
    content: c.content,
    isInternal: c.isInternal,
    authorName: c.authorName,
  }))

  return [...historyEntries, ...assignmentEntries, ...commentEntries].sort(
    (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
  )
}

import { MentionText } from '@/components/tickets/MentionText'
import type { TimelineEntry } from '@/utils/timeline'

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function TimelineRow({ entry }: { entry: TimelineEntry }) {
  if (entry.type === 'history') {
    return (
      <li className="text-sm">
        <p>
          <span className="font-medium">{entry.fieldName}</span>{' '}
          {entry.oldValue ? (
            <>
              changed from <span className="text-muted-foreground">{entry.oldValue}</span> to{' '}
              <span className="text-muted-foreground">{entry.newValue}</span>
            </>
          ) : (
            <span className="text-muted-foreground">{entry.newValue}</span>
          )}
        </p>
        <p className="text-xs text-muted-foreground">
          {entry.actorName ?? 'System'} · {formatDate(entry.timestamp)}
        </p>
      </li>
    )
  }

  if (entry.type === 'assignment') {
    const verb = entry.previousAssignedToName && entry.previousAssignedToName !== 'Unassigned' ? 'Reassigned' : 'Assigned'
    return (
      <li className="text-sm">
        <p>
          <span className="font-medium">{verb}</span> to{' '}
          <span className="text-muted-foreground">{entry.assignedToName ?? 'Unassigned'}</span>
          {entry.assignmentType === 'RoundRobin' && <span className="text-muted-foreground"> (round robin)</span>}
        </p>
        <p className="text-xs text-muted-foreground">
          {entry.assignedByName ?? 'System (Round Robin)'} · {formatDate(entry.timestamp)}
        </p>
      </li>
    )
  }

  return (
    <li className="text-sm">
      <p>
        <span className="font-medium">{entry.isInternal ? 'Internal note' : 'Comment'}:</span>{' '}
        <MentionText content={entry.content} />
      </p>
      <p className="text-xs text-muted-foreground">
        {entry.authorName} · {formatDate(entry.timestamp)}
      </p>
    </li>
  )
}

export function TicketTimeline({ entries }: { entries: TimelineEntry[] }) {
  if (entries.length === 0) {
    return <p className="text-sm text-muted-foreground">No activity recorded yet.</p>
  }

  return (
    <ul className="space-y-3">
      {entries.map((entry) => (
        <TimelineRow key={`${entry.type}-${entry.id}`} entry={entry} />
      ))}
    </ul>
  )
}

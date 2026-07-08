import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { useAssignTicketMutation, useAutoAssignTicketMutation } from '@/features/assignments/queries'
import { useAgentsQuery } from '@/features/lookups/queries'
import type { TicketDetail } from '@/types/tickets'
import { extractErrorMessage } from '@/utils/errors'

const selectClassName =
  'h-9 rounded-md border border-input bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

interface AssignmentPanelProps {
  ticket: TicketDetail
}

export function AssignmentPanel({ ticket }: AssignmentPanelProps) {
  const [selectedAgentId, setSelectedAgentId] = useState('')
  const [error, setError] = useState<string | null>(null)

  const { data: agents } = useAgentsQuery(true)
  const assignTicket = useAssignTicketMutation(ticket.id)
  const autoAssignTicket = useAutoAssignTicketMutation(ticket.id)

  const isPending = assignTicket.isPending || autoAssignTicket.isPending

  const handleAssign = async () => {
    if (!selectedAgentId) return
    setError(null)
    try {
      await assignTicket.mutateAsync(selectedAgentId)
      setSelectedAgentId('')
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleUnassign = async () => {
    setError(null)
    try {
      await assignTicket.mutateAsync(null)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleAutoAssign = async () => {
    setError(null)
    try {
      await autoAssignTicket.mutateAsync()
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <section className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">Assignment</h2>
      <p className="text-sm text-muted-foreground">
        Currently assigned to: <span className="text-foreground">{ticket.assignedToName ?? 'Unassigned'}</span>
      </p>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <div className="flex flex-wrap items-center gap-2">
        <select
          aria-label="Select an agent to assign"
          value={selectedAgentId}
          onChange={(e) => setSelectedAgentId(e.target.value)}
          className={selectClassName}
        >
          <option value="">Select an agent...</option>
          {agents?.map((agent) => (
            <option key={agent.id} value={agent.id}>
              {agent.name}
            </option>
          ))}
        </select>

        <Button size="sm" disabled={!selectedAgentId || isPending} onClick={() => void handleAssign()}>
          {ticket.assignedToUserId ? 'Reassign' : 'Assign'}
        </Button>

        <Button variant="outline" size="sm" disabled={isPending} onClick={() => void handleAutoAssign()}>
          Auto-assign (round robin)
        </Button>

        {ticket.assignedToUserId && (
          <Button variant="outline" size="sm" disabled={isPending} onClick={() => void handleUnassign()}>
            Unassign
          </Button>
        )}
      </div>
    </section>
  )
}

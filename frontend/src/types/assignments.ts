export interface AssignmentHistoryEntry {
  id: string
  previousAssignedToName: string | null
  assignedToName: string | null
  assignedByName: string | null
  assignmentType: 'Manual' | 'RoundRobin'
  assignedAt: string
}

import { useRef, useState } from 'react'
import { attachmentsApi } from '@/features/attachments/api'
import { useAttachmentsQuery, useDeleteAttachmentMutation, useUploadAttachmentMutation } from '@/features/attachments/queries'
import { useAuth } from '@/hooks/useAuth'
import type { TicketAttachment } from '@/types/attachments'
import { extractErrorMessage } from '@/utils/errors'
import { isAgentOrAbove } from '@/utils/roles'

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

export function AttachmentsPanel({ ticketId }: { ticketId: string }) {
  const { user } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { data: attachments, isLoading } = useAttachmentsQuery(ticketId)
  const uploadMutation = useUploadAttachmentMutation(ticketId)
  const deleteMutation = useDeleteAttachmentMutation(ticketId)

  const canDelete = (attachment: TicketAttachment) =>
    attachment.uploadedByUserId === user?.id || isAgentOrAbove(user?.roles ?? [])

  const handleFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    setError(null)
    try {
      await uploadMutation.mutateAsync(file)
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
    }
  }

  const handleDownload = async (attachment: TicketAttachment) => {
    setError(null)
    try {
      const blob = await attachmentsApi.download(ticketId, attachment.id)
      downloadBlob(blob, attachment.fileName)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleDelete = async (attachmentId: string) => {
    if (!window.confirm('Delete this attachment?')) return

    setError(null)
    try {
      await deleteMutation.mutateAsync(attachmentId)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <section className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">Attachments</h2>
      {error && <p className="text-sm text-destructive">{error}</p>}

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading attachments...</p>
      ) : !attachments || attachments.length === 0 ? (
        <p className="text-sm text-muted-foreground">No attachments yet.</p>
      ) : (
        <ul className="space-y-2">
          {attachments.map((attachment) => (
            <li key={attachment.id} className="flex items-center justify-between rounded-md bg-muted/30 p-2 text-sm">
              <div>
                <button
                  type="button"
                  onClick={() => void handleDownload(attachment)}
                  className="font-medium text-primary hover:underline"
                >
                  {attachment.fileName}
                </button>
                <p className="text-xs text-muted-foreground">
                  {formatBytes(attachment.fileSizeBytes)} · {attachment.uploadedByName} ·{' '}
                  {new Date(attachment.createdAt).toLocaleString()}
                </p>
              </div>
              {canDelete(attachment) && (
                <button
                  type="button"
                  onClick={() => void handleDelete(attachment.id)}
                  className="text-xs text-destructive hover:underline"
                >
                  Delete
                </button>
              )}
            </li>
          ))}
        </ul>
      )}

      <div>
        <input
          ref={fileInputRef}
          type="file"
          onChange={(e) => void handleFileSelected(e)}
          disabled={uploadMutation.isPending}
          className="text-sm"
        />
        {uploadMutation.isPending && <p className="mt-1 text-xs text-muted-foreground">Uploading...</p>}
      </div>
    </section>
  )
}

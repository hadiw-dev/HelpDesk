import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { useAdminLookupQueries } from '@/features/admin/lookups/queries'
import type { LookupResource } from '@/features/admin/lookups/api'
import type { AdminLookupItem } from '@/types/admin'
import { extractErrorMessage } from '@/utils/errors'

const inputClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

interface LookupManagementPageProps {
  resource: LookupResource
  title: string
}

const emptyForm = { name: '', description: '', displayOrder: 0, isActive: true }

export function LookupManagementPage({ resource, title }: LookupManagementPageProps) {
  const { listQuery, createMutation, updateMutation, deleteMutation } = useAdminLookupQueries(resource)
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const startEdit = (item: AdminLookupItem) => {
    setEditingId(item.id)
    setForm({ name: item.name, description: item.description ?? '', displayOrder: item.displayOrder, isActive: item.isActive })
  }

  const cancelEdit = () => {
    setEditingId(null)
    setForm(emptyForm)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      if (editingId) {
        await updateMutation.mutateAsync({ id: editingId, data: form })
      } else {
        await createMutation.mutateAsync(form)
      }
      cancelEdit()
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Delete "${name}"? Existing tickets keep their (now-hidden) reference.`)) return
    setError(null)
    try {
      await deleteMutation.mutateAsync(id)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const isSaving = createMutation.isPending || updateMutation.isPending

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">{title}</h1>
        <p className="text-sm text-muted-foreground">Deleting soft-hides a value; tickets already using it keep their reference.</p>
      </div>

      <form onSubmit={(e) => void handleSubmit(e)} className="space-y-3 rounded-xl border border-border p-4">
        <h2 className="text-sm font-semibold">{editingId ? 'Edit' : 'Add New'}</h2>
        {error && <p className="text-sm text-destructive">{error}</p>}
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          <input
            required
            aria-label="Name"
            placeholder="Name"
            className={inputClassName}
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
          <input
            aria-label="Description"
            placeholder="Description"
            className={inputClassName}
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
          />
          <input
            type="number"
            aria-label="Display order"
            placeholder="Display order"
            className={inputClassName}
            value={form.displayOrder}
            onChange={(e) => setForm({ ...form, displayOrder: Number(e.target.value) })}
          />
          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            />
            Active
          </label>
        </div>
        <div className="flex gap-2">
          <Button type="submit" size="sm" disabled={isSaving}>
            {editingId ? 'Save changes' : 'Add'}
          </Button>
          {editingId && (
            <Button type="button" variant="outline" size="sm" onClick={cancelEdit}>
              Cancel
            </Button>
          )}
        </div>
      </form>

      {listQuery.isLoading && <p className="text-sm text-muted-foreground">Loading...</p>}
      {listQuery.isError && <p className="text-sm text-destructive">Failed to load.</p>}

      {listQuery.data && (
        <div className="overflow-x-auto rounded-xl border border-border">
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/40 text-left text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Name</th>
                <th className="px-3 py-2">Description</th>
                <th className="px-3 py-2">Order</th>
                <th className="px-3 py-2">Active</th>
                <th className="px-3 py-2 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {listQuery.data.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-3 py-6 text-center text-muted-foreground">
                    Nothing here yet.
                  </td>
                </tr>
              ) : (
                listQuery.data.map((item) => (
                  <tr key={item.id} className="border-b border-border last:border-0">
                    <td className="px-3 py-2">{item.name}</td>
                    <td className="px-3 py-2 text-muted-foreground">{item.description ?? '—'}</td>
                    <td className="px-3 py-2">{item.displayOrder}</td>
                    <td className="px-3 py-2">{item.isActive ? 'Yes' : 'No'}</td>
                    <td className="px-3 py-2 text-right">
                      <button type="button" onClick={() => startEdit(item)} className="mr-3 text-xs text-primary hover:underline">
                        Edit
                      </button>
                      <button
                        type="button"
                        onClick={() => void handleDelete(item.id, item.name)}
                        className="text-xs text-destructive hover:underline"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

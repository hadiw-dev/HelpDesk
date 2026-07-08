import { useState } from 'react'
import { Button, buttonVariants } from '@/components/ui/button'
import {
  useChangeUserRoleMutation,
  useCreateUserMutation,
  useAdminUsersQuery,
  useDeleteUserMutation,
  useUpdateUserMutation,
} from '@/features/admin/users/queries'
import type { AdminUser } from '@/types/admin'
import { extractErrorMessage } from '@/utils/errors'
import { cn } from '@/lib/utils'

const ROLES = ['Employee', 'IT Support Agent', 'Manager', 'Admin']

const inputClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'
const selectClassName =
  'h-9 rounded-md border border-input bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

function CreateUserForm({ onCreated }: { onCreated: () => void }) {
  const createUser = useCreateUserMutation()
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '', role: 'Employee' })
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await createUser.mutateAsync(form)
      setForm({ email: '', password: '', firstName: '', lastName: '', role: 'Employee' })
      onCreated()
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <form onSubmit={(e) => void handleSubmit(e)} className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">Create User</h2>
      {error && <p className="text-sm text-destructive">{error}</p>}
      <div className="grid grid-cols-2 gap-3">
        <input
          required
          aria-label="First name"
          placeholder="First name"
          className={inputClassName}
          value={form.firstName}
          onChange={(e) => setForm({ ...form, firstName: e.target.value })}
        />
        <input
          required
          aria-label="Last name"
          placeholder="Last name"
          className={inputClassName}
          value={form.lastName}
          onChange={(e) => setForm({ ...form, lastName: e.target.value })}
        />
        <input
          required
          type="email"
          aria-label="Email"
          placeholder="Email"
          className={inputClassName}
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
        />
        <input
          required
          type="password"
          aria-label="Password"
          placeholder="Password"
          className={inputClassName}
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
        />
        <select
          aria-label="Role"
          className={selectClassName}
          value={form.role}
          onChange={(e) => setForm({ ...form, role: e.target.value })}
        >
          {ROLES.map((role) => (
            <option key={role} value={role}>
              {role}
            </option>
          ))}
        </select>
      </div>
      <Button type="submit" disabled={createUser.isPending}>
        {createUser.isPending ? 'Creating...' : 'Create User'}
      </Button>
    </form>
  )
}

function UserRow({ user }: { user: AdminUser }) {
  const changeRole = useChangeUserRoleMutation()
  const updateUser = useUpdateUserMutation(user.id)
  const deleteUser = useDeleteUserMutation()
  const [error, setError] = useState<string | null>(null)
  const [isEditing, setIsEditing] = useState(false)
  const [editForm, setEditForm] = useState({
    firstName: user.firstName,
    lastName: user.lastName,
    department: user.department ?? '',
    jobTitle: user.jobTitle ?? '',
  })

  const primaryRole = user.roles[0] ?? 'Employee'

  const handleRoleChange = async (role: string) => {
    setError(null)
    try {
      await changeRole.mutateAsync({ id: user.id, role })
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleToggleActive = async () => {
    setError(null)
    try {
      await updateUser.mutateAsync({
        firstName: user.firstName,
        lastName: user.lastName,
        department: user.department ?? undefined,
        jobTitle: user.jobTitle ?? undefined,
        isActive: !user.isActive,
      })
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleSaveEdit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await updateUser.mutateAsync({ ...editForm, isActive: user.isActive })
      setIsEditing(false)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  const handleDelete = async () => {
    if (!window.confirm(`Delete ${user.email}? This can't be undone from the UI.`)) return
    setError(null)
    try {
      await deleteUser.mutateAsync(user.id)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <>
      <tr className="border-b border-border last:border-0">
        <td className="px-3 py-2">
          {user.email}
          {error && <p className="text-xs text-destructive">{error}</p>}
        </td>
        <td className="px-3 py-2">
          {user.firstName} {user.lastName}
        </td>
        <td className="px-3 py-2">
          <select
            aria-label={`Change role for ${user.email}`}
            value={primaryRole}
            onChange={(e) => void handleRoleChange(e.target.value)}
            disabled={changeRole.isPending}
            className={selectClassName}
          >
            {ROLES.map((role) => (
              <option key={role} value={role}>
                {role}
              </option>
            ))}
          </select>
        </td>
        <td className="px-3 py-2">
          <button
            type="button"
            onClick={() => void handleToggleActive()}
            disabled={updateUser.isPending}
            className={cn('rounded px-2 py-0.5 text-xs', user.isActive ? 'bg-emerald-500/10 text-emerald-600' : 'bg-muted text-muted-foreground')}
          >
            {user.isActive ? 'Active' : 'Inactive'}
          </button>
        </td>
        <td className="px-3 py-2 text-right">
          <button type="button" onClick={() => setIsEditing((v) => !v)} className="mr-3 text-xs text-primary hover:underline">
            Edit
          </button>
          <button type="button" onClick={() => void handleDelete()} className="text-xs text-destructive hover:underline">
            Delete
          </button>
        </td>
      </tr>
      {isEditing && (
        <tr className="border-b border-border bg-muted/20 last:border-0">
          <td colSpan={5} className="px-3 py-3">
            <form onSubmit={(e) => void handleSaveEdit(e)} className="flex flex-wrap items-end gap-2">
              <input
                aria-label={`First name for ${user.email}`}
                placeholder="First name"
                className={inputClassName}
                value={editForm.firstName}
                onChange={(e) => setEditForm({ ...editForm, firstName: e.target.value })}
              />
              <input
                aria-label={`Last name for ${user.email}`}
                placeholder="Last name"
                className={inputClassName}
                value={editForm.lastName}
                onChange={(e) => setEditForm({ ...editForm, lastName: e.target.value })}
              />
              <input
                aria-label={`Department for ${user.email}`}
                placeholder="Department"
                className={inputClassName}
                value={editForm.department}
                onChange={(e) => setEditForm({ ...editForm, department: e.target.value })}
              />
              <input
                aria-label={`Job title for ${user.email}`}
                placeholder="Job title"
                className={inputClassName}
                value={editForm.jobTitle}
                onChange={(e) => setEditForm({ ...editForm, jobTitle: e.target.value })}
              />
              <Button type="submit" size="sm" disabled={updateUser.isPending}>
                Save
              </Button>
            </form>
          </td>
        </tr>
      )}
    </>
  )
}

export function UserManagementPage() {
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [roleFilter, setRoleFilter] = useState('')

  const { data, isLoading, isError } = useAdminUsersQuery({
    searchTerm: searchTerm || undefined,
    role: roleFilter || undefined,
    pageSize: 50,
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">User Management</h1>
        <p className="text-sm text-muted-foreground">Create accounts and manage roles/status.</p>
      </div>

      <CreateUserForm onCreated={() => undefined} />

      <div className="flex flex-wrap items-center gap-2">
        <form
          className="flex items-center gap-2"
          onSubmit={(e) => {
            e.preventDefault()
            setSearchTerm(searchInput)
          }}
        >
          <input
            aria-label="Search users by name or email"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search by name or email..."
            className={cn(inputClassName, 'w-64')}
          />
          <button type="submit" className={buttonVariants({ variant: 'secondary', size: 'sm' })}>
            Search
          </button>
        </form>
        <select
          aria-label="Filter by role"
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
          className={selectClassName}
        >
          <option value="">All roles</option>
          {ROLES.map((role) => (
            <option key={role} value={role}>
              {role}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-sm text-muted-foreground">Loading users...</p>}
      {isError && <p className="text-sm text-destructive">Failed to load users.</p>}

      {data && (
        <div className="overflow-x-auto rounded-xl border border-border">
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/40 text-left text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-3 py-2">Email</th>
                <th className="px-3 py-2">Name</th>
                <th className="px-3 py-2">Role</th>
                <th className="px-3 py-2">Status</th>
                <th className="px-3 py-2 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-3 py-6 text-center text-muted-foreground">
                    No users found.
                  </td>
                </tr>
              ) : (
                data.items.map((user) => <UserRow key={user.id} user={user} />)
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

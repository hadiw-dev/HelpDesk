import { Link } from 'react-router-dom'

const sections = [
  { to: '/admin/users', label: 'User Management', description: 'Create accounts, assign roles, activate/deactivate.' },
  { to: '/admin/categories', label: 'Category Management', description: 'Manage ticket categories.' },
  { to: '/admin/priorities', label: 'Priority Management', description: 'Manage ticket priorities.' },
  { to: '/admin/statuses', label: 'Status Management', description: 'Manage ticket workflow statuses.' },
  { to: '/admin/settings', label: 'System Settings', description: 'Site name, file upload limits, default page size.' },
  { to: '/admin/activity-logs', label: 'Activity Log', description: 'System-wide audit trail.' },
]

export function AdminPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Admin Dashboard</h1>
        <p className="text-sm text-muted-foreground">System administration — Admin role only.</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <Link
            key={section.to}
            to={section.to}
            className="space-y-1 rounded-xl border border-border p-4 transition-colors hover:bg-muted/40"
          >
            <h2 className="text-sm font-semibold">{section.label}</h2>
            <p className="text-xs text-muted-foreground">{section.description}</p>
          </Link>
        ))}
      </div>
    </div>
  )
}

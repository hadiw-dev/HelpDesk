import { cn } from '@/lib/utils'

interface KpiCardProps {
  label: string
  value: string | number
  accent?: 'default' | 'warning' | 'danger'
}

export function KpiCard({ label, value, accent = 'default' }: KpiCardProps) {
  return (
    <div className="rounded-xl border border-border p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p
        className={cn(
          'mt-1 text-2xl font-semibold tracking-tight',
          accent === 'danger' && 'text-destructive',
          accent === 'warning' && 'text-amber-600 dark:text-amber-500',
        )}
      >
        {value}
      </p>
    </div>
  )
}

import { CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { MonthlyTicketsItem } from '@/types/dashboard'

export function MonthlyTicketsLineChart({ data }: { data: MonthlyTicketsItem[] }) {
  return (
    <section className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">Monthly Tickets</h2>
      {data.length === 0 ? (
        <p className="text-sm text-muted-foreground">No tickets recorded yet.</p>
      ) : (
        <div className="h-64">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="monthLabel" fontSize={12} />
              <YAxis allowDecimals={false} fontSize={12} />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="createdCount" name="Created" stroke="#6366f1" strokeWidth={2} />
              <Line type="monotone" dataKey="resolvedCount" name="Resolved" stroke="#22c55e" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </section>
  )
}

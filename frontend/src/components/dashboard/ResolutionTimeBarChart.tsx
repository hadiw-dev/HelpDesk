import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { ResolutionTimeReport } from '@/types/dashboard'

export function ResolutionTimeBarChart({ data }: { data: ResolutionTimeReport }) {
  const chartData = data.byPriority.map((p) => ({
    priorityName: p.priorityName,
    averageHours: p.averageResolutionHours ?? 0,
    ticketCount: p.ticketCount,
  }))

  return (
    <section className="space-y-3 rounded-xl border border-border p-4">
      <h2 className="text-sm font-semibold">Resolution Time by Priority</h2>
      <p className="text-sm text-muted-foreground">
        Overall average: {data.overallAverageResolutionHours !== null ? `${data.overallAverageResolutionHours.toFixed(1)}h` : '—'}
      </p>
      {chartData.length === 0 ? (
        <p className="text-sm text-muted-foreground">No resolved tickets yet.</p>
      ) : (
        <div className="h-64">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="priorityName" fontSize={12} />
              <YAxis allowDecimals={false} fontSize={12} unit="h" />
              <Tooltip formatter={(value) => `${Number(value).toFixed(1)}h`} />
              <Bar dataKey="averageHours" name="Avg. resolution (hours)" fill="#6366f1" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </section>
  )
}

import { useMemo, useState } from 'react'
import { Button } from '@/components/ui/button'
import { DateRangeFilter } from '@/components/dashboard/DateRangeFilter'
import { dashboardApi } from '@/features/dashboard/api'
import { extractErrorMessage } from '@/utils/errors'

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

export function ReportsPage() {
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [isExportingPdf, setIsExportingPdf] = useState(false)
  const [isExportingExcel, setIsExportingExcel] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const params = useMemo(
    () => ({ dateFrom: dateFrom || undefined, dateTo: dateTo || undefined }),
    [dateFrom, dateTo],
  )

  const handleExportPdf = async () => {
    setError(null)
    setIsExportingPdf(true)
    try {
      const blob = await dashboardApi.getPdfReport(params)
      downloadBlob(blob, `dashboard-report-${new Date().toISOString().slice(0, 10)}.pdf`)
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setIsExportingPdf(false)
    }
  }

  const handleExportExcel = async () => {
    setError(null)
    setIsExportingExcel(true)
    try {
      const blob = await dashboardApi.getExcelReport(params)
      downloadBlob(blob, `dashboard-report-${new Date().toISOString().slice(0, 10)}.xlsx`)
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setIsExportingExcel(false)
    }
  }

  return (
    <div className="max-w-xl space-y-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Reports</h1>
        <p className="text-sm text-muted-foreground">
          Export the same KPI, category/priority breakdown, monthly trend, resolution time, and SLA data shown on the
          Dashboard as a document.
        </p>
      </div>

      <div className="space-y-3 rounded-xl border border-border p-4">
        <h2 className="text-sm font-semibold">Date range (optional)</h2>
        <DateRangeFilter
          dateFrom={dateFrom}
          dateTo={dateTo}
          onChange={(next) => {
            setDateFrom(next.dateFrom)
            setDateTo(next.dateTo)
          }}
        />
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <div className="flex flex-wrap gap-3">
        <Button onClick={() => void handleExportPdf()} disabled={isExportingPdf}>
          {isExportingPdf ? 'Generating PDF...' : 'Export PDF'}
        </Button>
        <Button variant="outline" onClick={() => void handleExportExcel()} disabled={isExportingExcel}>
          {isExportingExcel ? 'Generating Excel...' : 'Export Excel'}
        </Button>
      </div>
    </div>
  )
}

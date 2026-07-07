interface DateRangeFilterProps {
  dateFrom: string
  dateTo: string
  onChange: (next: { dateFrom: string; dateTo: string }) => void
}

const inputClassName =
  'h-9 rounded-md border border-input bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

export function DateRangeFilter({ dateFrom, dateTo, onChange }: DateRangeFilterProps) {
  return (
    <div className="flex flex-wrap items-center gap-2">
      <label className="flex items-center gap-2 text-sm text-muted-foreground">
        From
        <input
          type="date"
          value={dateFrom}
          onChange={(e) => onChange({ dateFrom: e.target.value, dateTo })}
          className={inputClassName}
        />
      </label>
      <label className="flex items-center gap-2 text-sm text-muted-foreground">
        To
        <input
          type="date"
          value={dateTo}
          onChange={(e) => onChange({ dateFrom, dateTo: e.target.value })}
          className={inputClassName}
        />
      </label>
      {(dateFrom || dateTo) && (
        <button
          type="button"
          onClick={() => onChange({ dateFrom: '', dateTo: '' })}
          className="text-xs text-primary hover:underline"
        >
          Clear
        </button>
      )}
    </div>
  )
}

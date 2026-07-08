export function PageLoadingFallback() {
  return (
    <div className="flex min-h-40 w-full items-center justify-center py-16">
      <div
        role="status"
        aria-label="Loading page"
        className="size-6 animate-spin rounded-full border-2 border-muted-foreground/30 border-t-foreground"
      />
    </div>
  )
}

import { isRouteErrorResponse, useRouteError } from 'react-router-dom'
import { buttonVariants } from '@/components/ui/button'

export function ErrorPage() {
  const error = useRouteError()

  const message = isRouteErrorResponse(error)
    ? `${error.status} ${error.statusText}`
    : error instanceof Error
      ? error.message
      : 'An unexpected error occurred.'

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-3 bg-background px-4 text-center text-foreground">
      <p className="text-sm font-medium text-muted-foreground">Error</p>
      <h1 className="text-2xl font-semibold tracking-tight">Something went wrong</h1>
      <p className="max-w-sm text-sm text-muted-foreground">{message}</p>
      <a href="/" className={buttonVariants({ variant: 'default' })}>
        Back to dashboard
      </a>
    </div>
  )
}

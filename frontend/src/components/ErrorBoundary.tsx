import { Component, type ErrorInfo, type ReactNode } from 'react'
import { buttonVariants } from '@/components/ui/button'

interface ErrorBoundaryProps {
  children: ReactNode
}

interface ErrorBoundaryState {
  error: Error | null
}

/**
 * Catches render-time errors that occur outside React Router's own data flow (route `errorElement`
 * only covers a route's element and its descendants) — e.g. a provider mounted above the router in
 * App.tsx. Without this, such an error would unmount the whole app to a blank screen.
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Unhandled error caught by ErrorBoundary:', error, errorInfo)
  }

  render() {
    if (this.state.error) {
      return (
        <div className="flex min-h-svh flex-col items-center justify-center gap-3 bg-background px-4 text-center text-foreground">
          <p className="text-sm font-medium text-muted-foreground">Error</p>
          <h1 className="text-2xl font-semibold tracking-tight">Something went wrong</h1>
          <p className="max-w-sm text-sm text-muted-foreground">{this.state.error.message}</p>
          <a href="/" className={buttonVariants({ variant: 'default' })}>
            Back to dashboard
          </a>
        </div>
      )
    }

    return this.props.children
  }
}

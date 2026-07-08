import { Suspense } from 'react'
import { Outlet } from 'react-router-dom'
import { Navbar } from '@/components/layout/Navbar'
import { PageLoadingFallback } from '@/components/PageLoadingFallback'

export function AppLayout() {
  return (
    <div className="min-h-svh bg-background text-foreground">
      <Navbar />
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Suspense fallback={<PageLoadingFallback />}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  )
}

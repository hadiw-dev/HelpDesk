import { RouterProvider } from 'react-router-dom'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { AuthProvider } from '@/features/auth/AuthContext'
import { QueryProvider } from '@/providers/QueryProvider'
import { ThemeProvider } from '@/providers/ThemeProvider'
import { router } from '@/routes/AppRoutes'

function App() {
  return (
    <ThemeProvider defaultTheme="system">
      <QueryProvider>
        <AuthProvider>
          <ErrorBoundary>
            <RouterProvider router={router} />
          </ErrorBoundary>
        </AuthProvider>
      </QueryProvider>
    </ThemeProvider>
  )
}

export default App

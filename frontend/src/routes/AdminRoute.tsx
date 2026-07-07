import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

/** Wraps `/admin/*` routes — `ProtectedRoute` already guarantees authentication by the time this runs. */
export function AdminRoute() {
  const { user } = useAuth()

  if (!user?.roles.includes('Admin')) {
    return <Navigate to="/dashboard" replace />
  }

  return <Outlet />
}

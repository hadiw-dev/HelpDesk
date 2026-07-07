import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '@/layouts/AppLayout'
import { AuthLayout } from '@/layouts/AuthLayout'
import { AdminPage } from '@/pages/AdminPage'
import { ActivityLogPage } from '@/pages/admin/ActivityLogPage'
import { CategoryManagementPage } from '@/pages/admin/CategoryManagementPage'
import { PriorityManagementPage } from '@/pages/admin/PriorityManagementPage'
import { StatusManagementPage } from '@/pages/admin/StatusManagementPage'
import { SystemSettingsPage } from '@/pages/admin/SystemSettingsPage'
import { UserManagementPage } from '@/pages/admin/UserManagementPage'
import { CreateTicketPage } from '@/pages/CreateTicketPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { EditTicketPage } from '@/pages/EditTicketPage'
import { ErrorPage } from '@/pages/ErrorPage'
import { ForgotPasswordPage } from '@/pages/ForgotPasswordPage'
import { LoginPage } from '@/pages/LoginPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ProfilePage } from '@/pages/ProfilePage'
import { RegisterPage } from '@/pages/RegisterPage'
import { ReportsPage } from '@/pages/ReportsPage'
import { ResetPasswordPage } from '@/pages/ResetPasswordPage'
import { TicketDetailsPage } from '@/pages/TicketDetailsPage'
import { TicketsPage } from '@/pages/TicketsPage'
import { AdminRoute } from '@/routes/AdminRoute'
import { ProtectedRoute } from '@/routes/ProtectedRoute'

export const router = createBrowserRouter([
  {
    element: <AuthLayout />,
    errorElement: <ErrorPage />,
    children: [
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
      { path: '/forgot-password', element: <ForgotPasswordPage /> },
      { path: '/reset-password', element: <ResetPasswordPage /> },
    ],
  },
  {
    element: <ProtectedRoute />,
    errorElement: <ErrorPage />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: '/', element: <Navigate to="/dashboard" replace /> },
          { path: '/dashboard', element: <DashboardPage /> },
          { path: '/tickets', element: <TicketsPage /> },
          { path: '/tickets/new', element: <CreateTicketPage /> },
          { path: '/tickets/:id', element: <TicketDetailsPage /> },
          { path: '/tickets/:id/edit', element: <EditTicketPage /> },
          { path: '/reports', element: <ReportsPage /> },
          { path: '/profile', element: <ProfilePage /> },
          {
            element: <AdminRoute />,
            children: [
              { path: '/admin', element: <AdminPage /> },
              { path: '/admin/users', element: <UserManagementPage /> },
              { path: '/admin/categories', element: <CategoryManagementPage /> },
              { path: '/admin/priorities', element: <PriorityManagementPage /> },
              { path: '/admin/statuses', element: <StatusManagementPage /> },
              { path: '/admin/settings', element: <SystemSettingsPage /> },
              { path: '/admin/activity-logs', element: <ActivityLogPage /> },
            ],
          },
        ],
      },
    ],
  },
  { path: '*', element: <NotFoundPage /> },
])

import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from '@/layouts/AppLayout'
import { AuthLayout } from '@/layouts/AuthLayout'
import { ErrorPage } from '@/pages/ErrorPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { AdminRoute } from '@/routes/AdminRoute'
import {
  ActivityLogPage,
  AdminPage,
  CategoryManagementPage,
  CreateTicketPage,
  DashboardPage,
  EditTicketPage,
  ForgotPasswordPage,
  LoginPage,
  PriorityManagementPage,
  ProfilePage,
  RegisterPage,
  ReportsPage,
  ResetPasswordPage,
  StatusManagementPage,
  SystemSettingsPage,
  TicketDetailsPage,
  TicketsPage,
  UserManagementPage,
} from '@/routes/lazyPages'
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

import { lazy } from 'react'

// Route-level code splitting: each page is only fetched when its route is actually visited, instead
// of every page's code shipping in the initial bundle. Named exports need the `.then(...)` wrapper
// because `React.lazy` requires a default export. Kept in their own module (rather than inline in
// AppRoutes.tsx) so this file only exports components — mixing them with the non-component `router`
// export trips up react-refresh's Fast Refresh detection.
export const LoginPage = lazy(() => import('@/pages/LoginPage').then((m) => ({ default: m.LoginPage })))
export const RegisterPage = lazy(() => import('@/pages/RegisterPage').then((m) => ({ default: m.RegisterPage })))
export const ForgotPasswordPage = lazy(() =>
  import('@/pages/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage })),
)
export const ResetPasswordPage = lazy(() =>
  import('@/pages/ResetPasswordPage').then((m) => ({ default: m.ResetPasswordPage })),
)
export const DashboardPage = lazy(() => import('@/pages/DashboardPage').then((m) => ({ default: m.DashboardPage })))
export const TicketsPage = lazy(() => import('@/pages/TicketsPage').then((m) => ({ default: m.TicketsPage })))
export const CreateTicketPage = lazy(() =>
  import('@/pages/CreateTicketPage').then((m) => ({ default: m.CreateTicketPage })),
)
export const TicketDetailsPage = lazy(() =>
  import('@/pages/TicketDetailsPage').then((m) => ({ default: m.TicketDetailsPage })),
)
export const EditTicketPage = lazy(() =>
  import('@/pages/EditTicketPage').then((m) => ({ default: m.EditTicketPage })),
)
export const ReportsPage = lazy(() => import('@/pages/ReportsPage').then((m) => ({ default: m.ReportsPage })))
export const ProfilePage = lazy(() => import('@/pages/ProfilePage').then((m) => ({ default: m.ProfilePage })))
export const AdminPage = lazy(() => import('@/pages/AdminPage').then((m) => ({ default: m.AdminPage })))
export const UserManagementPage = lazy(() =>
  import('@/pages/admin/UserManagementPage').then((m) => ({ default: m.UserManagementPage })),
)
export const CategoryManagementPage = lazy(() =>
  import('@/pages/admin/CategoryManagementPage').then((m) => ({ default: m.CategoryManagementPage })),
)
export const PriorityManagementPage = lazy(() =>
  import('@/pages/admin/PriorityManagementPage').then((m) => ({ default: m.PriorityManagementPage })),
)
export const StatusManagementPage = lazy(() =>
  import('@/pages/admin/StatusManagementPage').then((m) => ({ default: m.StatusManagementPage })),
)
export const SystemSettingsPage = lazy(() =>
  import('@/pages/admin/SystemSettingsPage').then((m) => ({ default: m.SystemSettingsPage })),
)
export const ActivityLogPage = lazy(() =>
  import('@/pages/admin/ActivityLogPage').then((m) => ({ default: m.ActivityLogPage })),
)

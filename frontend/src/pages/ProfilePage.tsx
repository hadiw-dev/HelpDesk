import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { authApi } from '@/features/auth/api'
import {
  changePasswordSchema,
  updateProfileSchema,
  type ChangePasswordFormValues,
  type UpdateProfileFormValues,
} from '@/features/auth/schemas'
import { extractErrorMessage } from '@/utils/errors'

export function ProfilePage() {
  const { user, logout, refreshProfile } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  if (!user) {
    return null
  }

  return (
    <div className="space-y-8">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Profile</h1>
          <p className="text-sm text-muted-foreground">
            Signed in as {user.email} ({user.roles.join(', ') || 'no roles assigned'})
          </p>
        </div>
        <Button variant="outline" onClick={() => void handleLogout()}>
          Log out
        </Button>
      </div>

      <ProfileDetailsForm />
      <ChangePasswordForm onSuccess={refreshProfile} />
    </div>
  )
}

function ProfileDetailsForm() {
  const { user, refreshProfile } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<UpdateProfileFormValues>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      firstName: user?.firstName ?? '',
      lastName: user?.lastName ?? '',
      department: user?.department ?? '',
      jobTitle: user?.jobTitle ?? '',
    },
  })

  const onSubmit = async (values: UpdateProfileFormValues) => {
    setServerError(null)
    setSuccess(false)
    try {
      await authApi.updateProfile(values)
      await refreshProfile()
      setSuccess(true)
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-md space-y-4 rounded-xl border border-border p-4" noValidate>
      <h2 className="text-sm font-semibold">Profile details</h2>

      {serverError && <p className="text-sm text-destructive">{serverError}</p>}
      {success && <p className="text-sm text-primary">Profile updated.</p>}

      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1">
          <label htmlFor="firstName" className="text-sm font-medium">
            First name
          </label>
          <input
            id="firstName"
            className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
            {...register('firstName')}
          />
          {errors.firstName && <p className="text-xs text-destructive">{errors.firstName.message}</p>}
        </div>
        <div className="space-y-1">
          <label htmlFor="lastName" className="text-sm font-medium">
            Last name
          </label>
          <input
            id="lastName"
            className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
            {...register('lastName')}
          />
          {errors.lastName && <p className="text-xs text-destructive">{errors.lastName.message}</p>}
        </div>
      </div>

      <div className="space-y-1">
        <label htmlFor="department" className="text-sm font-medium">
          Department
        </label>
        <input
          id="department"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('department')}
        />
      </div>

      <div className="space-y-1">
        <label htmlFor="jobTitle" className="text-sm font-medium">
          Job title
        </label>
        <input
          id="jobTitle"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('jobTitle')}
        />
      </div>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : 'Save changes'}
      </Button>
    </form>
  )
}

function ChangePasswordForm({ onSuccess }: { onSuccess: () => Promise<void> }) {
  const [serverError, setServerError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordFormValues>({ resolver: zodResolver(changePasswordSchema) })

  const onSubmit = async (values: ChangePasswordFormValues) => {
    setServerError(null)
    setSuccess(false)
    try {
      await authApi.changePassword(values)
      reset()
      setSuccess(true)
      await onSuccess()
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-md space-y-4 rounded-xl border border-border p-4" noValidate>
      <h2 className="text-sm font-semibold">Change password</h2>

      {serverError && <p className="text-sm text-destructive">{serverError}</p>}
      {success && <p className="text-sm text-primary">Password changed.</p>}

      <div className="space-y-1">
        <label htmlFor="currentPassword" className="text-sm font-medium">
          Current password
        </label>
        <input
          id="currentPassword"
          type="password"
          autoComplete="current-password"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('currentPassword')}
        />
        {errors.currentPassword && <p className="text-xs text-destructive">{errors.currentPassword.message}</p>}
      </div>

      <div className="space-y-1">
        <label htmlFor="newPassword" className="text-sm font-medium">
          New password
        </label>
        <input
          id="newPassword"
          type="password"
          autoComplete="new-password"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('newPassword')}
        />
        {errors.newPassword && <p className="text-xs text-destructive">{errors.newPassword.message}</p>}
      </div>

      <div className="space-y-1">
        <label htmlFor="confirmPassword" className="text-sm font-medium">
          Confirm new password
        </label>
        <input
          id="confirmPassword"
          type="password"
          autoComplete="new-password"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('confirmPassword')}
        />
        {errors.confirmPassword && <p className="text-xs text-destructive">{errors.confirmPassword.message}</p>}
      </div>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Changing...' : 'Change password'}
      </Button>
    </form>
  )
}

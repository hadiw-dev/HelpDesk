import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { authApi } from '@/features/auth/api'
import { resetPasswordSchema, type ResetPasswordFormValues } from '@/features/auth/schemas'
import { extractErrorMessage } from '@/utils/errors'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: searchParams.get('email') ?? '',
      token: searchParams.get('token') ?? '',
    },
  })

  const onSubmit = async (values: ResetPasswordFormValues) => {
    setServerError(null)
    try {
      await authApi.resetPassword(values)
      navigate('/login', { replace: true })
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
      <div>
        <h2 className="text-base font-semibold">Reset password</h2>
        <p className="text-sm text-muted-foreground">
          Enter the reset link details along with your new password.
        </p>
      </div>

      {serverError && <p className="text-sm text-destructive">{serverError}</p>}

      <div className="space-y-1">
        <label htmlFor="email" className="text-sm font-medium">
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('email')}
        />
        {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
      </div>

      <div className="space-y-1">
        <label htmlFor="token" className="text-sm font-medium">
          Reset token
        </label>
        <textarea
          id="token"
          rows={3}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
          {...register('token')}
        />
        {errors.token && <p className="text-xs text-destructive">{errors.token.message}</p>}
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

      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting ? 'Resetting...' : 'Reset password'}
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        <Link to="/login" className="text-primary underline-offset-4 hover:underline">
          Back to sign in
        </Link>
      </p>
    </form>
  )
}

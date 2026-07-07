import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { authApi } from '@/features/auth/api'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '@/features/auth/schemas'
import { extractErrorMessage } from '@/utils/errors'

export function ForgotPasswordPage() {
  const [serverError, setServerError] = useState<string | null>(null)
  const [submitted, setSubmitted] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormValues>({ resolver: zodResolver(forgotPasswordSchema) })

  const onSubmit = async (values: ForgotPasswordFormValues) => {
    setServerError(null)
    try {
      await authApi.forgotPassword(values)
      setSubmitted(true)
    } catch (error) {
      setServerError(extractErrorMessage(error))
    }
  }

  if (submitted) {
    return (
      <div className="space-y-4 text-center">
        <h2 className="text-base font-semibold">Check your email</h2>
        <p className="text-sm text-muted-foreground">
          If an account exists for that email, a password reset link has been sent.
        </p>
        <Link to="/login" className="text-sm text-primary underline-offset-4 hover:underline">
          Back to sign in
        </Link>
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
      <div>
        <h2 className="text-base font-semibold">Forgot password</h2>
        <p className="text-sm text-muted-foreground">Enter your email and we&apos;ll send you a reset link.</p>
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

      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting ? 'Sending...' : 'Send reset link'}
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        <Link to="/login" className="text-primary underline-offset-4 hover:underline">
          Back to sign in
        </Link>
      </p>
    </form>
  )
}

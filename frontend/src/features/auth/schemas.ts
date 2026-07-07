import { z } from 'zod'

const passwordSchema = z
  .string()
  .min(8, 'Must be at least 8 characters.')
  .regex(/[A-Z]/, 'Must contain at least one uppercase letter.')
  .regex(/[a-z]/, 'Must contain at least one lowercase letter.')
  .regex(/[0-9]/, 'Must contain at least one digit.')

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
  password: z.string().min(1, 'Password is required.'),
})
export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z
  .object({
    firstName: z.string().min(1, 'First name is required.').max(100),
    lastName: z.string().min(1, 'Last name is required.').max(100),
    email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })
export type RegisterFormValues = z.infer<typeof registerSchema>

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
})
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>

export const resetPasswordSchema = z
  .object({
    email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
    token: z.string().min(1, 'Reset token is required.'),
    newPassword: passwordSchema,
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required.'),
    newPassword: passwordSchema,
    confirmPassword: z.string().min(1, 'Please confirm your new password.'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })
export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>

export const updateProfileSchema = z.object({
  firstName: z.string().min(1, 'First name is required.').max(100),
  lastName: z.string().min(1, 'Last name is required.').max(100),
  department: z.string().max(100).optional().or(z.literal('')),
  jobTitle: z.string().max(100).optional().or(z.literal('')),
})
export type UpdateProfileFormValues = z.infer<typeof updateProfileSchema>

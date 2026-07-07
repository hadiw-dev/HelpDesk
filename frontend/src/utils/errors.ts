import { AxiosError } from 'axios'

interface ProblemDetailsBody {
  title?: string
  errors?: Record<string, string[]>
}

export function extractErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as ProblemDetailsBody | undefined

    const firstValidationError = data?.errors && Object.values(data.errors)[0]?.[0]
    if (firstValidationError) {
      return firstValidationError
    }

    if (data?.title) {
      return data.title
    }
  }

  return 'Something went wrong. Please try again.'
}

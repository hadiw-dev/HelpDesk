import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { API_BASE_URL } from '@/utils/constants'
import { tokenStorage } from '@/utils/tokenStorage'

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

export const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

axiosInstance.interceptors.request.use((config) => {
  const token = tokenStorage.getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let refreshPromise: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = tokenStorage.getRefreshToken()
  if (!refreshToken) {
    return null
  }

  try {
    const response = await axios.post<{ accessToken: string; refreshToken: string }>(
      `${API_BASE_URL}/auth/refresh-token`,
      { refreshToken },
    )
    tokenStorage.setTokens(response.data.accessToken, response.data.refreshToken)
    return response.data.accessToken
  } catch {
    tokenStorage.clear()
    return null
  }
}

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined

    const isAuthEndpoint = originalRequest?.url?.includes('/auth/login') || originalRequest?.url?.includes('/auth/refresh-token')

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint && tokenStorage.getRefreshToken()) {
      originalRequest._retry = true

      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null
      })

      const newAccessToken = await refreshPromise

      if (newAccessToken) {
        originalRequest.headers.set('Authorization', `Bearer ${newAccessToken}`)
        return axiosInstance(originalRequest)
      }

      if (typeof window !== 'undefined') {
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  },
)

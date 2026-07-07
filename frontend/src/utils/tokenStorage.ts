import { ACCESS_TOKEN_STORAGE_KEY, REFRESH_TOKEN_STORAGE_KEY } from '@/utils/constants'

export const tokenStorage = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY),
  setAccessToken: (token: string) => localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, token),

  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY),
  setRefreshToken: (token: string) => localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, token),

  setTokens: (accessToken: string, refreshToken: string) => {
    localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, accessToken)
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, refreshToken)
  },

  clear: () => {
    localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY)
    localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY)
  },
}

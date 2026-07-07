import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi, type LoginInput, type RegisterInput } from '@/features/auth/api'
import type { UserProfile } from '@/types/auth'
import { tokenStorage } from '@/utils/tokenStorage'

export interface AuthContextValue {
  user: UserProfile | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (input: LoginInput) => Promise<void>
  register: (input: RegisterInput) => Promise<void>
  logout: () => Promise<void>
  refreshProfile: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const restoreSession = async () => {
      if (!tokenStorage.getRefreshToken()) {
        setIsLoading(false)
        return
      }

      try {
        const profile = await authApi.getProfile()
        setUser(profile)
      } catch {
        tokenStorage.clear()
        setUser(null)
      } finally {
        setIsLoading(false)
      }
    }

    void restoreSession()
  }, [])

  const login = useCallback(async (input: LoginInput) => {
    const result = await authApi.login(input)
    tokenStorage.setTokens(result.accessToken, result.refreshToken)
    setUser(result.user)
  }, [])

  const register = useCallback(async (input: RegisterInput) => {
    const result = await authApi.register(input)
    tokenStorage.setTokens(result.accessToken, result.refreshToken)
    setUser(result.user)
  }, [])

  const logout = useCallback(async () => {
    const refreshToken = tokenStorage.getRefreshToken()
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken)
      } catch {
        // Best-effort: proceed with clearing local session regardless.
      }
    }
    tokenStorage.clear()
    setUser(null)
  }, [])

  const refreshProfile = useCallback(async () => {
    const profile = await authApi.getProfile()
    setUser(profile)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      isLoading,
      login,
      register,
      logout,
      refreshProfile,
    }),
    [user, isLoading, login, register, logout, refreshProfile],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

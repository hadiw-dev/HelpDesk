import { NavLink, useNavigate } from 'react-router-dom'
import { LogOut, Moon, Sun } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { NotificationCenter } from '@/components/notifications/NotificationCenter'
import { useAuth } from '@/hooks/useAuth'
import { useTheme } from '@/providers/ThemeProvider'
import { cn } from '@/lib/utils'

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/tickets', label: 'Tickets' },
  { to: '/reports', label: 'Reports' },
  { to: '/admin', label: 'Admin' },
  { to: '/profile', label: 'Profile' },
]

export function Navbar() {
  const { theme, setTheme } = useTheme()
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="border-b border-border bg-background">
      <div className="mx-auto flex h-14 max-w-6xl items-center justify-between px-4">
        <span className="text-sm font-semibold tracking-tight">HelpDesk System</span>

        <nav className="flex items-center gap-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                cn(
                  'rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground',
                  isActive && 'bg-muted text-foreground',
                )
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-1">
          {user && <span className="hidden text-xs text-muted-foreground sm:inline">{user.email}</span>}

          <NotificationCenter />

          <Button
            variant="ghost"
            size="icon"
            aria-label="Toggle theme"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
          >
            {theme === 'dark' ? <Sun /> : <Moon />}
          </Button>

          <Button variant="ghost" size="icon" aria-label="Log out" onClick={() => void handleLogout()}>
            <LogOut />
          </Button>
        </div>
      </div>
    </header>
  )
}

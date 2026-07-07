import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Bell } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  useMarkAllNotificationsReadMutation,
  useMarkNotificationReadMutation,
  useNotificationsQuery,
  useUnreadNotificationCountQuery,
} from '@/features/notifications/queries'
import type { NotificationItem } from '@/types/notifications'
import { cn } from '@/lib/utils'

export function NotificationCenter() {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()

  const { data: unreadCount } = useUnreadNotificationCountQuery()
  const { data } = useNotificationsQuery({ page: 1, pageSize: 10 })
  const markAsRead = useMarkNotificationReadMutation()
  const markAllAsRead = useMarkAllNotificationsReadMutation()

  useEffect(() => {
    function handleOutsideClick(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleOutsideClick)
    return () => document.removeEventListener('mousedown', handleOutsideClick)
  }, [])

  const handleNotificationClick = async (notification: NotificationItem) => {
    if (!notification.isRead) {
      await markAsRead.mutateAsync(notification.id)
    }
    setOpen(false)
    if (notification.relatedTicketId) {
      navigate(`/tickets/${notification.relatedTicketId}`)
    }
  }

  return (
    <div className="relative" ref={containerRef}>
      <Button variant="ghost" size="icon" aria-label="Notifications" onClick={() => setOpen((o) => !o)}>
        <Bell />
        {Boolean(unreadCount) && (
          <span className="absolute right-0.5 top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-semibold text-destructive-foreground">
            {unreadCount! > 9 ? '9+' : unreadCount}
          </span>
        )}
      </Button>

      {open && (
        <div className="absolute right-0 z-20 mt-2 w-80 rounded-md border border-border bg-background shadow-lg">
          <div className="flex items-center justify-between border-b border-border px-3 py-2">
            <span className="text-sm font-semibold">Notifications</span>
            <button
              type="button"
              className="text-xs text-primary hover:underline disabled:opacity-50"
              disabled={markAllAsRead.isPending || !unreadCount}
              onClick={() => void markAllAsRead.mutateAsync()}
            >
              Mark all read
            </button>
          </div>
          <ul className="max-h-96 overflow-y-auto">
            {!data || data.items.length === 0 ? (
              <li className="px-3 py-4 text-center text-sm text-muted-foreground">No notifications yet.</li>
            ) : (
              data.items.map((notification) => (
                <li key={notification.id}>
                  <button
                    type="button"
                    onClick={() => void handleNotificationClick(notification)}
                    className={cn(
                      'block w-full border-b border-border px-3 py-2 text-left text-sm last:border-0 hover:bg-muted/50',
                      !notification.isRead && 'bg-muted/30',
                    )}
                  >
                    <p className="font-medium">{notification.title}</p>
                    <p className="text-xs text-muted-foreground">{notification.message}</p>
                    <p className="mt-1 text-[11px] text-muted-foreground">
                      {new Date(notification.createdAt).toLocaleString()}
                    </p>
                  </button>
                </li>
              ))
            )}
          </ul>
        </div>
      )}
    </div>
  )
}

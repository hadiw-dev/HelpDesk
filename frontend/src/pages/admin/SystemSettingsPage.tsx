import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Button } from '@/components/ui/button'
import { useSystemSettingsQuery, useUpdateSystemSettingsMutation } from '@/features/admin/settings/queries'
import type { SystemSettings } from '@/types/admin'
import { extractErrorMessage } from '@/utils/errors'

const inputClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50'

export function SystemSettingsPage() {
  const { data: settings, isLoading } = useSystemSettingsQuery()
  const updateSettings = useUpdateSystemSettingsMutation()
  const [error, setError] = useState<string | null>(null)
  const [saved, setSaved] = useState(false)

  const { register, handleSubmit } = useForm<SystemSettings>({ values: settings })

  const onSubmit = async (values: SystemSettings) => {
    setError(null)
    setSaved(false)
    try {
      await updateSettings.mutateAsync({
        ...values,
        maxFileUploadSizeMb: Number(values.maxFileUploadSizeMb),
        defaultPageSize: Number(values.defaultPageSize),
      })
      setSaved(true)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">Loading settings...</p>
  }

  return (
    <div className="max-w-xl space-y-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">System Settings</h1>
        <p className="text-sm text-muted-foreground">
          The file upload limit and allowed extensions here are enforced live by every ticket attachment upload.
        </p>
      </div>

      <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="space-y-4">
        {error && <p className="text-sm text-destructive">{error}</p>}
        {saved && <p className="text-sm text-emerald-600">Settings saved.</p>}

        <div className="space-y-1">
          <label htmlFor="siteName" className="text-sm font-medium">
            Site name
          </label>
          <input id="siteName" className={inputClassName} {...register('siteName')} />
        </div>

        <div className="space-y-1">
          <label htmlFor="maxFileUploadSizeMb" className="text-sm font-medium">
            Max file upload size (MB)
          </label>
          <input
            id="maxFileUploadSizeMb"
            type="number"
            min={1}
            max={100}
            className={inputClassName}
            {...register('maxFileUploadSizeMb')}
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="allowedFileExtensions" className="text-sm font-medium">
            Allowed file extensions (comma-separated)
          </label>
          <input id="allowedFileExtensions" className={inputClassName} {...register('allowedFileExtensions')} />
        </div>

        <div className="space-y-1">
          <label htmlFor="defaultPageSize" className="text-sm font-medium">
            Default page size
          </label>
          <input
            id="defaultPageSize"
            type="number"
            min={1}
            max={100}
            className={inputClassName}
            {...register('defaultPageSize')}
          />
        </div>

        <Button type="submit" disabled={updateSettings.isPending}>
          {updateSettings.isPending ? 'Saving...' : 'Save settings'}
        </Button>
      </form>
    </div>
  )
}

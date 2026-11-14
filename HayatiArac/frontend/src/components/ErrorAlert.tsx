import { Alert, AlertDescription } from '@/components/ui/alert'
import { ApiError } from '@/lib/api/errors'

// Hata mesajı her zaman düz metin olarak render edilir (React escape eder).
export function ErrorAlert({ error }: { error: unknown }) {
  if (!error) return null
  const message = error instanceof ApiError ? error.message : 'Beklenmeyen bir hata oluştu.'

  return (
    <Alert variant="destructive" role="alert">
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  )
}

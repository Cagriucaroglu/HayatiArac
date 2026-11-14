import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { ApiError } from './errors'

// Backend'in 400 ValidationProblemDetails içindeki alan hatalarını (ör. "phoneNumber")
// aynı isimli form alanlarının altına yazar. Eşleşen alan bulunduysa true döner.
export function applyServerFieldErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
): boolean {
  if (!(error instanceof ApiError)) return false

  let applied = false
  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    const field = fields.find((name) => name === key)
    const message = messages[0]
    if (field && message) {
      setError(field, { type: 'server', message })
      applied = true
    }
  }
  return applied
}

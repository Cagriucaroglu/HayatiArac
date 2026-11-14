import type { ReactNode } from 'react'
import { Label } from '@/components/ui/label'

type FormFieldProps = {
  id: string
  label: string
  error?: string | undefined
  children: ReactNode
}

// Input'a aria-invalid ve aria-describedby={`${id}-error`} verilmeli;
// böylece ekran okuyucular hata mesajını alanla birlikte okur.
export function FormField({ id, label, error, children }: FormFieldProps) {
  return (
    <div className="grid gap-2">
      <Label htmlFor={id}>{label}</Label>
      {children}
      {error && (
        <p id={`${id}-error`} className="text-sm text-destructive">
          {error}
        </p>
      )}
    </div>
  )
}

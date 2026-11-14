import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useNavigate } from 'react-router'
import { ErrorAlert } from '@/components/ErrorAlert'
import { FormField } from '@/components/FormField'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { ApiError } from '@/lib/api/errors'
import { applyServerFieldErrors } from '@/lib/api/formErrors'
import { useAuth } from '@/lib/auth/useAuth'
import { fieldA11y } from '@/lib/fieldA11y'
import { register as registerUser } from '../api'
import { savePendingVerification } from '../pendingVerification'
import { registerSchema, type RegisterInput } from '../schemas'

const SERVER_FIELDS = [
  'firstName',
  'lastName',
  'email',
  'phoneCountryCode',
  'phoneNumber',
  'password',
] as const

// Çakışma hataları ilgili alanın altında gösterilir.
const CONFLICT_FIELDS: Record<string, (typeof SERVER_FIELDS)[number]> = {
  'Auth.EmailAlreadyExists': 'email',
  'Auth.PhoneAlreadyExists': 'phoneNumber',
}

export function RegisterPage() {
  const navigate = useNavigate()
  const { status } = useAuth()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterInput>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      phoneCountryCode: '90',
      phoneNumber: '',
      password: '',
      confirmPassword: '',
    },
  })

  const mutation = useMutation({
    mutationFn: registerUser,
    onSuccess: (data) => {
      savePendingVerification({ userId: data.userId, email: data.email })
      void navigate('/dogrula', { replace: true })
    },
    onError: (error) => {
      const conflictField = error instanceof ApiError && error.code ? CONFLICT_FIELDS[error.code] : undefined
      if (conflictField) {
        setError(conflictField, { type: 'server', message: error.message })
        return
      }
      applyServerFieldErrors(error, setError, SERVER_FIELDS)
    },
  })

  if (status === 'authenticated') return <Navigate to="/" replace />

  // Alan bazlı gösterilen hatalar üstte tekrar edilmez.
  const showTopError =
    mutation.error instanceof ApiError &&
    !(mutation.error.code && CONFLICT_FIELDS[mutation.error.code]) &&
    Object.keys(mutation.error.fieldErrors).length === 0

  return (
    <Card className="mx-auto w-full max-w-md">
      <CardHeader>
        <CardTitle>Kayıt ol</CardTitle>
        <CardDescription>
          Kayıttan sonra e-posta adresinize ve telefonunuza doğrulama kodu gönderilir.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="grid gap-4"
          noValidate
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          {showTopError && <ErrorAlert error={mutation.error} />}

          <div className="grid gap-4 sm:grid-cols-2">
            <FormField id="firstName" label="Ad" error={errors.firstName?.message}>
              <Input
                autoComplete="given-name"
                {...fieldA11y('firstName', errors.firstName?.message)}
                {...register('firstName')}
              />
            </FormField>
            <FormField id="lastName" label="Soyad" error={errors.lastName?.message}>
              <Input
                autoComplete="family-name"
                {...fieldA11y('lastName', errors.lastName?.message)}
                {...register('lastName')}
              />
            </FormField>
          </div>

          <FormField id="email" label="E-posta" error={errors.email?.message}>
            <Input
              type="email"
              autoComplete="email"
              {...fieldA11y('email', errors.email?.message)}
              {...register('email')}
            />
          </FormField>

          <div className="grid grid-cols-[5rem_1fr] gap-4">
            <FormField id="phoneCountryCode" label="Ülke kodu" error={errors.phoneCountryCode?.message}>
              <Input
                inputMode="numeric"
                autoComplete="tel-country-code"
                {...fieldA11y('phoneCountryCode', errors.phoneCountryCode?.message)}
                {...register('phoneCountryCode')}
              />
            </FormField>
            <FormField id="phoneNumber" label="Telefon" error={errors.phoneNumber?.message}>
              <Input
                type="tel"
                inputMode="numeric"
                autoComplete="tel-national"
                placeholder="5551234567"
                {...fieldA11y('phoneNumber', errors.phoneNumber?.message)}
                {...register('phoneNumber')}
              />
            </FormField>
          </div>

          <FormField id="password" label="Şifre" error={errors.password?.message}>
            <Input
              type="password"
              autoComplete="new-password"
              {...fieldA11y('password', errors.password?.message)}
              {...register('password')}
            />
          </FormField>

          <FormField
            id="confirmPassword"
            label="Şifre (tekrar)"
            error={errors.confirmPassword?.message}
          >
            <Input
              type="password"
              autoComplete="new-password"
              {...fieldA11y('confirmPassword', errors.confirmPassword?.message)}
              {...register('confirmPassword')}
            />
          </FormField>

          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Kaydediliyor…' : 'Kayıt ol'}
          </Button>

          <p className="text-center text-sm text-muted-foreground">
            Zaten hesabınız var mı?{' '}
            <Link to="/giris" className="text-foreground underline underline-offset-4">
              Giriş yapın
            </Link>
          </p>
        </form>
      </CardContent>
    </Card>
  )
}

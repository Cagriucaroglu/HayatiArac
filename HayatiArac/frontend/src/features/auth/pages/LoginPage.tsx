import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useSearchParams } from 'react-router'
import { ErrorAlert } from '@/components/ErrorAlert'
import { FormField } from '@/components/FormField'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { signIn } from '@/lib/auth/session'
import { useAuth } from '@/lib/auth/useAuth'
import { fieldA11y } from '@/lib/fieldA11y'
import { safeRedirect } from '@/lib/safeRedirect'
import { login } from '../api'
import { loginSchema, type LoginInput } from '../schemas'

export function LoginPage() {
  const [searchParams] = useSearchParams()
  const redirectTo = safeRedirect(searchParams.get('redirect'))
  const { status } = useAuth()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
    defaultValues: { emailOrUsername: '', password: '' },
  })

  const mutation = useMutation({ mutationFn: login, onSuccess: signIn })

  // Giriş başarılı olunca (veya zaten oturum varsa) güvenli dönüş adresine git.
  if (status === 'authenticated') return <Navigate to={redirectTo} replace />

  return (
    <Card className="mx-auto w-full max-w-sm">
      <CardHeader>
        <CardTitle>Giriş yap</CardTitle>
        <CardDescription>E-posta adresiniz veya kullanıcı adınızla giriş yapın.</CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="grid gap-4"
          noValidate
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <ErrorAlert error={mutation.error} />

          <FormField
            id="emailOrUsername"
            label="E-posta veya kullanıcı adı"
            error={errors.emailOrUsername?.message}
          >
            <Input
              autoComplete="username"
              {...fieldA11y('emailOrUsername', errors.emailOrUsername?.message)}
              {...register('emailOrUsername')}
            />
          </FormField>

          <FormField id="password" label="Şifre" error={errors.password?.message}>
            <Input
              type="password"
              autoComplete="current-password"
              {...fieldA11y('password', errors.password?.message)}
              {...register('password')}
            />
          </FormField>

          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Giriş yapılıyor…' : 'Giriş yap'}
          </Button>

          <p className="text-center text-sm text-muted-foreground">
            Hesabınız yok mu?{' '}
            <Link to="/kayit" className="text-foreground underline underline-offset-4">
              Kayıt olun
            </Link>
          </p>
        </form>
      </CardContent>
    </Card>
  )
}

import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { ErrorAlert } from '@/components/ErrorAlert'
import { FormField } from '@/components/FormField'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ApiError } from '@/lib/api/errors'
import { fieldA11y } from '@/lib/fieldA11y'
import { useCountdown } from '@/lib/useCountdown'
import { resendOtp, verifyOtp } from '../api'
import { otpSchema, type OtpInput } from '../schemas'
import type { OtpChannel } from '../types'

const RESEND_COOLDOWN_SECONDS = 60

const ALREADY_VERIFIED_CODES = new Set(['Auth.EmailAlreadyVerified', 'Auth.PhoneAlreadyVerified'])

type OtpVerifyFormProps = {
  channel: OtpChannel
  userId: string
  label: string
  onVerified: () => void
}

export function OtpVerifyForm({ channel, userId, label, onVerified }: OtpVerifyFormProps) {
  const id = `otp-${channel}`
  // Kod kayıt sırasında gönderildi; tekrar gönderim hemen değil, bekleme süresinden sonra açılır.
  const cooldown = useCountdown(RESEND_COOLDOWN_SECONDS)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<OtpInput>({
    resolver: zodResolver(otpSchema),
    defaultValues: { otpCode: '' },
  })

  const verify = useMutation({
    mutationFn: (values: OtpInput) => verifyOtp(channel, userId, values.otpCode),
    onSuccess: onVerified,
    onError: (error) => {
      if (error instanceof ApiError && error.code && ALREADY_VERIFIED_CODES.has(error.code)) {
        onVerified()
      }
    },
  })

  const resend = useMutation({
    mutationFn: () => resendOtp(channel, userId),
    onSuccess: () => {
      reset()
      verify.reset()
      cooldown.restart()
    },
  })

  return (
    <form
      className="grid gap-3"
      noValidate
      onSubmit={handleSubmit((values) => verify.mutate(values))}
    >
      <ErrorAlert error={verify.error ?? resend.error} />
      {resend.isSuccess && (
        <p className="text-sm text-muted-foreground" role="status">
          Yeni kod gönderildi.
        </p>
      )}

      <FormField id={id} label={label} error={errors.otpCode?.message}>
        <Input
          inputMode="numeric"
          autoComplete="one-time-code"
          maxLength={6}
          placeholder="123456"
          {...fieldA11y(id, errors.otpCode?.message)}
          {...register('otpCode')}
        />
      </FormField>

      <div className="flex flex-wrap items-center gap-2">
        <Button type="submit" disabled={verify.isPending}>
          {verify.isPending ? 'Doğrulanıyor…' : 'Doğrula'}
        </Button>
        <Button
          type="button"
          variant="ghost"
          disabled={cooldown.remaining > 0 || resend.isPending}
          onClick={() => resend.mutate()}
        >
          {cooldown.remaining > 0 ? `Tekrar gönder (${cooldown.remaining} sn)` : 'Kodu tekrar gönder'}
        </Button>
      </div>
    </form>
  )
}

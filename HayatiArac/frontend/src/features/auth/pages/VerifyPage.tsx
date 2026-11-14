import { CircleCheck } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { OtpVerifyForm } from '../components/OtpVerifyForm'
import { clearPendingVerification, readPendingVerification } from '../pendingVerification'
import type { OtpChannel } from '../types'

export function VerifyPage() {
  const [pending] = useState(readPendingVerification)
  const [verified, setVerified] = useState<Record<OtpChannel, boolean>>({ email: false, phone: false })
  const allVerified = verified.email && verified.phone

  useEffect(() => {
    if (allVerified) clearPendingVerification()
  }, [allVerified])

  if (!pending) {
    return (
      <Card className="mx-auto w-full max-w-md">
        <CardHeader>
          <CardTitle>Doğrulama bilgisi bulunamadı</CardTitle>
          <CardDescription>
            Doğrulama sayfası kayıttan hemen sonra, aynı sekmede açılır. Yeni bir kayıt oluşturabilir
            ya da hesabınız doğrulanmışsa giriş yapabilirsiniz.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex gap-2">
          <Button asChild>
            <Link to="/kayit">Kayıt ol</Link>
          </Button>
          <Button variant="outline" asChild>
            <Link to="/giris">Giriş yap</Link>
          </Button>
        </CardContent>
      </Card>
    )
  }

  const markVerified = (channel: OtpChannel) => () =>
    setVerified((current) => ({ ...current, [channel]: true }))

  return (
    <Card className="mx-auto w-full max-w-md">
      <CardHeader>
        <CardTitle>Hesabınızı doğrulayın</CardTitle>
        <CardDescription>
          {pending.email} adresine ve telefonunuza gönderilen 6 haneli kodları girin.
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-6">
        {verified.email ? (
          <VerifiedNote text="E-posta adresiniz doğrulandı." />
        ) : (
          <OtpVerifyForm
            channel="email"
            userId={pending.userId}
            label="E-posta doğrulama kodu"
            onVerified={markVerified('email')}
          />
        )}

        {verified.phone ? (
          <VerifiedNote text="Telefon numaranız doğrulandı." />
        ) : (
          <OtpVerifyForm
            channel="phone"
            userId={pending.userId}
            label="SMS doğrulama kodu"
            onVerified={markVerified('phone')}
          />
        )}

        {allVerified && (
          <Button asChild>
            <Link to="/giris">Giriş yap</Link>
          </Button>
        )}
      </CardContent>
    </Card>
  )
}

function VerifiedNote({ text }: { text: string }) {
  return (
    <p className="flex items-center gap-2 text-sm" role="status">
      <CircleCheck className="size-4 text-green-600" aria-hidden="true" />
      {text}
    </p>
  )
}

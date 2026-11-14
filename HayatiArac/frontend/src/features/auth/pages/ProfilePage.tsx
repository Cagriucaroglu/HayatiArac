import { useQuery } from '@tanstack/react-query'
import { ErrorAlert } from '@/components/ErrorAlert'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { getCurrentUser } from '../api'

const dateFormatter = new Intl.DateTimeFormat('tr-TR', { dateStyle: 'long' })

export function ProfilePage() {
  const { data, isPending, error } = useQuery({
    queryKey: ['users', 'me'],
    queryFn: ({ signal }) => getCurrentUser(signal),
  })

  if (isPending) return <p className="text-muted-foreground">Yükleniyor…</p>
  if (error) return <ErrorAlert error={error} />

  const rows: [string, string | null][] = [
    ['Ad soyad', `${data.firstName} ${data.lastName}`],
    ['E-posta', data.email],
    ['Telefon', data.phoneNumber],
    ['Şehir', [data.city, data.district].filter(Boolean).join(' / ') || null],
    ['Üyelik tarihi', dateFormatter.format(new Date(data.createdAt))],
  ]

  return (
    <Card className="mx-auto w-full max-w-lg">
      <CardHeader>
        <CardTitle>Profilim</CardTitle>
      </CardHeader>
      <CardContent>
        <dl className="grid gap-3 sm:grid-cols-[10rem_1fr]">
          {rows.map(([label, value]) => (
            <div key={label} className="contents">
              <dt className="text-sm text-muted-foreground">{label}</dt>
              <dd className="text-sm">{value ?? '—'}</dd>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  )
}

import { Link } from 'react-router'
import { Button } from '@/components/ui/button'

export function NotFoundPage() {
  return (
    <section className="grid justify-items-start gap-4">
      <h1 className="text-2xl font-semibold">Sayfa bulunamadı</h1>
      <p className="text-muted-foreground">Aradığınız sayfa taşınmış ya da hiç var olmamış olabilir.</p>
      <Button asChild>
        <Link to="/">Ana sayfaya dön</Link>
      </Button>
    </section>
  )
}

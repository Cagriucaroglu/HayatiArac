import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '@/lib/auth/useAuth'

// UI seviyesinde koruma: oturum yoksa login'e yönlendirir. Asıl yetki kontrolü backend'dedir.
export function ProtectedRoute() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'loading') return <p className="text-muted-foreground">Yükleniyor…</p>

  if (status === 'anonymous') {
    const redirect = encodeURIComponent(`${location.pathname}${location.search}`)
    return <Navigate to={`/giris?redirect=${redirect}`} replace />
  }

  return <Outlet />
}

import { useMutation } from '@tanstack/react-query'
import { Link, Outlet, useNavigate } from 'react-router'
import { Button } from '@/components/ui/button'
import { logout } from '@/features/auth/api'
import { useAuth } from '@/lib/auth/useAuth'

export function AppLayout() {
  const auth = useAuth()
  const navigate = useNavigate()
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => navigate('/', { replace: true }),
  })

  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b">
        <div className="mx-auto flex h-14 w-full max-w-5xl items-center justify-between px-4">
          <Link to="/" className="font-semibold">
            HayatiAraç
          </Link>

          <nav className="flex items-center gap-2">
            {auth.status === 'authenticated' && (
              <>
                <span className="hidden text-sm text-muted-foreground sm:inline">
                  {auth.session.user.userName}
                </span>
                <Button variant="ghost" asChild>
                  <Link to="/profil">Profil</Link>
                </Button>
                <Button
                  variant="outline"
                  disabled={logoutMutation.isPending}
                  onClick={() => logoutMutation.mutate()}
                >
                  Çıkış
                </Button>
              </>
            )}
            {auth.status === 'anonymous' && (
              <>
                <Button variant="ghost" asChild>
                  <Link to="/giris">Giriş</Link>
                </Button>
                <Button asChild>
                  <Link to="/kayit">Kayıt ol</Link>
                </Button>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}

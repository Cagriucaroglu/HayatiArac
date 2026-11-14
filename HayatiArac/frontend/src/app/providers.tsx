import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useEffect, type ReactNode } from 'react'
import { ApiError } from '@/lib/api/errors'
import { authStore } from '@/lib/auth/authStore'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      // 4xx hatalar tekrar denemekle düzelmez; yalnızca ağ/sunucu hatalarında en fazla 2 kez denenir.
      retry: (failureCount, error) =>
        !(error instanceof ApiError && error.status >= 400 && error.status < 500) && failureCount < 2,
    },
  },
})

export function AppProviders({ children }: { children: ReactNode }) {
  // Oturum kapanınca (logout, refresh başarısız, başka sekmede çıkış) önceki kullanıcının
  // cache'teki verisi bir sonraki kullanıcıya görünmesin diye tamamen temizlenir.
  useEffect(
    () =>
      authStore.subscribe(() => {
        if (authStore.getSnapshot().status === 'anonymous') queryClient.clear()
      }),
    [],
  )

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

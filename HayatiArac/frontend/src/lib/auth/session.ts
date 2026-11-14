import { authStore } from './authStore'

// Login ve refresh endpoint'lerinin web istemcisine döndüğü gövde.
// Refresh token burada yok: backend onu HttpOnly cookie olarak set eder.
export type AuthTokenResponse = {
  userId: string
  userName: string
  email: string
  role: string
  accessToken: string
  accessTokenExpiresAt: string
}

const REFRESH_URL = '/api/users/refresh'
const REFRESH_LOCK = 'hayatiarac-auth-refresh'
const CHANNEL_NAME = 'hayatiarac-auth'

let inflightRefresh: Promise<boolean> | null = null
let channel: BroadcastChannel | null = null

function getChannel(): BroadcastChannel | null {
  if (!channel && typeof BroadcastChannel !== 'undefined') {
    channel = new BroadcastChannel(CHANNEL_NAME)
  }
  return channel
}

// .NET DateTime saat dilimi bilgisi olmadan gelirse UTC kabul edilir.
function parseUtc(value: string): number {
  return Date.parse(/(Z|[+-]\d\d:\d\d)$/i.test(value) ? value : `${value}Z`)
}

function applyTokenResponse(data: AuthTokenResponse) {
  authStore.setSession({
    user: {
      userId: data.userId,
      userName: data.userName,
      email: data.email,
      role: data.role,
    },
    accessToken: data.accessToken,
    expiresAt: parseUtc(data.accessTokenExpiresAt),
  })
}

async function requestRefresh(): Promise<boolean> {
  let response: Response
  try {
    response = await fetch(REFRESH_URL, {
      method: 'POST',
      credentials: 'same-origin',
      headers: { Accept: 'application/json' },
    })
  } catch {
    // Ağ hatası: oturumun geçersiz olduğu kanıtlanmadı, mevcut duruma dokunma.
    return false
  }

  if (!response.ok) {
    authStore.clear()
    return false
  }

  applyTokenResponse((await response.json()) as AuthTokenResponse)
  return true
}

// Sekmeler arası kilit: iki sekme aynı cookie ile aynı anda refresh yaparsa ikincisi
// rotate edilmiş (iptal) token'ı gönderir ve backend reuse detection ile tüm oturumları kapatır.
// Kilit sayesinde ikinci sekme, birincinin yazdığı yeni cookie ile istek atar.
async function withCrossTabLock(task: () => Promise<boolean>): Promise<boolean> {
  if (typeof navigator !== 'undefined' && 'locks' in navigator) {
    return await navigator.locks.request(REFRESH_LOCK, task)
  }
  return await task()
}

// Single-flight: aynı anda gelen tüm çağrılar tek bir refresh isteğini paylaşır.
export function refreshSession(): Promise<boolean> {
  inflightRefresh ??= withCrossTabLock(requestRefresh).finally(() => {
    inflightRefresh = null
  })
  return inflightRefresh
}

// Uygulama açılışında bir kez çağrılır: cookie geçerliyse oturum geri gelir.
export async function initSession() {
  getChannel()?.addEventListener('message', (event: MessageEvent) => {
    if (event.data === 'logout') authStore.clear()
    if (event.data === 'login' && authStore.getSnapshot().status !== 'authenticated') {
      void refreshSession()
    }
  })

  await refreshSession()
  if (authStore.getSnapshot().status !== 'authenticated') authStore.clear()
}

export function signIn(data: AuthTokenResponse) {
  applyTokenResponse(data)
  getChannel()?.postMessage('login')
}

export function endSession() {
  authStore.clear()
  getChannel()?.postMessage('logout')
}

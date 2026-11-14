import { authStore } from '@/lib/auth/authStore'
import { refreshSession } from '@/lib/auth/session'
import { ApiError, toApiError } from './errors'

// Token süresinin dolmasına bu kadar kala istekten önce yenilenir.
const PROACTIVE_REFRESH_MS = 60_000

export type ApiRequestOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  // Access token yalnızca kendi API'mize gider; dışarıdan gelen bir path ile başka origin'e sızamaz.
  if (!path.startsWith('/api/')) {
    throw new Error(`apiRequest yalnızca /api/ altındaki yolları kabul eder: ${path}`)
  }

  await refreshIfExpiring()

  const first = await send(path, options)
  let response = first.response

  if (response.status === 401 && first.token) {
    // Başka bir istek bu arada token'ı yenilemiş olabilir; o zaman tekrar refresh yapmadan dene.
    const tokenChanged = currentToken() !== first.token
    if (tokenChanged || (await refreshSession())) {
      response = (await send(path, options)).response
    }
  }

  if (!response.ok) throw await toApiError(response)

  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

function currentToken(): string | null {
  return authStore.getSnapshot().session?.accessToken ?? null
}

async function refreshIfExpiring() {
  const { session } = authStore.getSnapshot()
  if (session && session.expiresAt - Date.now() < PROACTIVE_REFRESH_MS) {
    await refreshSession()
  }
}

async function send(path: string, { method = 'GET', body, signal }: ApiRequestOptions) {
  const token = currentToken()
  const headers = new Headers({ Accept: 'application/json' })
  if (body !== undefined) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  try {
    const response = await fetch(path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
      credentials: 'same-origin',
    })
    return { response, token }
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error
    throw new ApiError(0, null)
  }
}

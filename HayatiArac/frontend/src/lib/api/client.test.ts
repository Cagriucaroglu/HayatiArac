import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { authStore } from '@/lib/auth/authStore'
import { apiRequest } from './client'
import { ApiError } from './errors'

const user = { userId: 'u1', userName: 'test', email: 'test@test.com', role: 'User' }
const FIFTEEN_MINUTES = 15 * 60_000

function json(status: number, body?: unknown) {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

function tokenResponse(accessToken: string) {
  return {
    ...user,
    accessToken,
    accessTokenExpiresAt: new Date(Date.now() + FIFTEEN_MINUTES).toISOString(),
  }
}

function authHeader(init?: RequestInit) {
  return new Headers(init?.headers).get('Authorization')
}

const fetchMock = vi.fn<typeof fetch>()

// /api/users/refresh → yeni token; diğer istekler yalnızca yeni token ile 200, aksi halde 401.
function serverWithExpiredToken(refreshDelayMs = 0) {
  fetchMock.mockImplementation(async (input, init) => {
    if (String(input) === '/api/users/refresh') {
      await new Promise((resolve) => setTimeout(resolve, refreshDelayMs))
      return json(200, tokenResponse('new-token'))
    }
    return authHeader(init) === 'Bearer new-token' ? json(200, { ok: true }) : json(401)
  })
}

function refreshCalls() {
  return fetchMock.mock.calls.filter(([input]) => String(input) === '/api/users/refresh').length
}

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
  authStore.setSession({ user, accessToken: 'old-token', expiresAt: Date.now() + FIFTEEN_MINUTES })
})

afterEach(() => {
  vi.unstubAllGlobals()
  authStore.clear()
})

describe('apiRequest', () => {
  it('access token Authorization header ile gönderilir', async () => {
    fetchMock.mockImplementation(async () => json(200, { ok: true }))

    await apiRequest('/api/test')

    expect(authHeader(fetchMock.mock.calls[0]?.[1])).toBe('Bearer old-token')
  })

  it('401 alınca token yenilenir ve istek bir kez tekrarlanır', async () => {
    serverWithExpiredToken()

    await expect(apiRequest('/api/test')).resolves.toEqual({ ok: true })

    expect(refreshCalls()).toBe(1)
    expect(authStore.getSnapshot().session?.accessToken).toBe('new-token')
  })

  it('aynı anda 401 alan istekler tek bir refresh çağrısını paylaşır', async () => {
    serverWithExpiredToken(20)

    const results = await Promise.all([
      apiRequest('/api/a'),
      apiRequest('/api/b'),
      apiRequest('/api/c'),
    ])

    expect(results).toEqual([{ ok: true }, { ok: true }, { ok: true }])
    expect(refreshCalls()).toBe(1)
  })

  it('refresh başarısızsa oturum kapanır ve 401 hatası fırlatılır', async () => {
    fetchMock.mockImplementation(async (input) =>
      String(input) === '/api/users/refresh'
        ? json(401, { detail: 'Oturum bulunamadı.', code: 'Auth.MissingRefreshToken' })
        : json(401),
    )

    await expect(apiRequest('/api/test')).rejects.toMatchObject({ status: 401 })

    expect(authStore.getSnapshot().status).toBe('anonymous')
  })

  it('süresi dolmak üzere olan token istekten önce yenilenir', async () => {
    authStore.setSession({ user, accessToken: 'old-token', expiresAt: Date.now() + 30_000 })
    serverWithExpiredToken()

    await apiRequest('/api/test')

    expect(String(fetchMock.mock.calls[0]?.[0])).toBe('/api/users/refresh')
    expect(authHeader(fetchMock.mock.calls[1]?.[1])).toBe('Bearer new-token')
  })

  it('/api/ dışındaki adreslere istek atmaz (token başka origin\'e gitmez)', async () => {
    await expect(apiRequest('https://evil.example.com/api/x')).rejects.toThrow()
    await expect(apiRequest('//evil.example.com/api/x')).rejects.toThrow()

    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('ProblemDetails gövdesi ApiError alanlarına dönüştürülür', async () => {
    fetchMock.mockImplementation(async () =>
      json(400, {
        detail: 'E-posta/kullanıcı adı veya şifre hatalı.',
        code: 'Auth.InvalidCredentials',
        errors: { email: ['Geçerli bir e-posta adresi giriniz.'] },
      }),
    )

    const error = await apiRequest('/api/test').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({
      status: 400,
      message: 'E-posta/kullanıcı adı veya şifre hatalı.',
      code: 'Auth.InvalidCredentials',
      fieldErrors: { email: ['Geçerli bir e-posta adresi giriniz.'] },
    })
  })

  it('ağ hatası status 0 olan ApiError olarak döner', async () => {
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'))

    await expect(apiRequest('/api/test')).rejects.toMatchObject({ status: 0 })
  })
})

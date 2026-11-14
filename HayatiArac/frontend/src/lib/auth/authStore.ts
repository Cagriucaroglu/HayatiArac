// Oturum durumu. Access token YALNIZCA bu modül değişkeninde (bellekte) tutulur;
// localStorage/sessionStorage'a asla yazılmaz. Sayfa yenilenince kaybolur ve
// HttpOnly refresh cookie'si ile /api/users/refresh üzerinden geri alınır.

export type AuthUser = {
  userId: string
  userName: string
  email: string
  role: string
}

export type Session = {
  user: AuthUser
  accessToken: string
  expiresAt: number
}

export type AuthState =
  | { status: 'loading'; session: null }
  | { status: 'anonymous'; session: null }
  | { status: 'authenticated'; session: Session }

let state: AuthState = { status: 'loading', session: null }
const listeners = new Set<() => void>()

function setState(next: AuthState) {
  state = next
  listeners.forEach((listener) => listener())
}

export const authStore = {
  getSnapshot: (): AuthState => state,

  subscribe(listener: () => void) {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },

  setSession(session: Session) {
    setState({ status: 'authenticated', session })
  },

  clear() {
    setState({ status: 'anonymous', session: null })
  },
}

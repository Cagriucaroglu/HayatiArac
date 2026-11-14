import { apiRequest } from '@/lib/api/client'
import { endSession, type AuthTokenResponse } from '@/lib/auth/session'
import type { LoginInput, RegisterInput } from './schemas'
import type { OtpChannel, RegisterResponse, UserProfile } from './types'

export function login(input: LoginInput) {
  return apiRequest<AuthTokenResponse>('/api/users/login', {
    method: 'POST',
    body: { ...input, rememberMe: false },
  })
}

// Gönderilen alanlar açıkça listelenir: confirmPassword gibi yalnızca formda kullanılan alanlar sunucuya gitmez.
export function register(input: RegisterInput) {
  const { email, password, firstName, lastName, phoneNumber, phoneCountryCode } = input
  return apiRequest<RegisterResponse>('/api/users/register', {
    method: 'POST',
    body: { email, password, firstName, lastName, phoneNumber, phoneCountryCode },
  })
}

export function verifyOtp(channel: OtpChannel, userId: string, otpCode: string) {
  return apiRequest<unknown>(`/api/users/verify-${channel}`, {
    method: 'POST',
    body: { userId, otpCode },
  })
}

export function resendOtp(channel: OtpChannel, userId: string) {
  return apiRequest<unknown>(`/api/users/resend-${channel}-otp`, {
    method: 'POST',
    body: { userId },
  })
}

export function getCurrentUser(signal?: AbortSignal) {
  return apiRequest<UserProfile>('/api/users/me', { signal })
}

// Sunucu çağrısı başarısız olsa bile (ağ hatası vb.) yerel oturum kapatılır.
export async function logout() {
  try {
    await apiRequest<unknown>('/api/users/logout', { method: 'POST' })
  } finally {
    endSession()
  }
}

import { describe, expect, it } from 'vitest'
import { otpSchema, registerSchema } from './schemas'

const validRegister = {
  firstName: 'Ayşe',
  lastName: 'Yılmaz',
  email: 'ayse@example.com',
  phoneCountryCode: '90',
  phoneNumber: '5551234567',
  password: 'gizli123',
  confirmPassword: 'gizli123',
}

describe('registerSchema', () => {
  it('geçerli kaydı kabul eder', () => {
    expect(registerSchema.safeParse(validRegister).success).toBe(true)
  })

  it.each(['05551234567', '555123456', '555-123-45'])('%s telefon numarasını reddeder', (phoneNumber) => {
    expect(registerSchema.safeParse({ ...validRegister, phoneNumber }).success).toBe(false)
  })

  it('şifreler eşleşmezse confirmPassword alanında hata verir', () => {
    const result = registerSchema.safeParse({ ...validRegister, confirmPassword: 'farkli123' })

    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.path).toEqual(['confirmPassword'])
  })

  it('6 karakterden kısa şifreyi reddeder', () => {
    const result = registerSchema.safeParse({ ...validRegister, password: '12345', confirmPassword: '12345' })

    expect(result.success).toBe(false)
  })
})

describe('otpSchema', () => {
  it.each(['12345', '1234567', 'abcdef', '12 345'])('%s kodunu reddeder', (otpCode) => {
    expect(otpSchema.safeParse({ otpCode }).success).toBe(false)
  })

  it('6 haneli kodu kabul eder', () => {
    expect(otpSchema.safeParse({ otpCode: '123456' }).success).toBe(true)
  })
})

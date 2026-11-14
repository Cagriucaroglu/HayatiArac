import { z } from 'zod'

// Kurallar backend validator'larını aynalar (RegisterUserCommandValidator vb.).
// Asıl doğrulama backend'dedir; bunlar kullanıcıya erken geri bildirim içindir.

export const loginSchema = z.object({
  emailOrUsername: z
    .string()
    .trim()
    .min(1, 'E-posta veya kullanıcı adı gereklidir.')
    .max(256, 'En fazla 256 karakter girilebilir.'),
  password: z.string().min(1, 'Şifre gereklidir.').max(128, 'Şifre en fazla 128 karakter olabilir.'),
})
export type LoginInput = z.infer<typeof loginSchema>

export const registerSchema = z
  .object({
    firstName: z.string().trim().min(1, 'Ad gereklidir.').max(50, 'Ad en fazla 50 karakter olabilir.'),
    lastName: z
      .string()
      .trim()
      .min(1, 'Soyad gereklidir.')
      .max(50, 'Soyad en fazla 50 karakter olabilir.'),
    email: z.email('Geçerli bir e-posta adresi girin.').max(256, 'E-posta en fazla 256 karakter olabilir.'),
    phoneCountryCode: z.string().regex(/^\d{1,4}$/, 'Ülke kodu geçersiz (örnek: 90).'),
    phoneNumber: z
      .string()
      .regex(/^\d{10}$/, 'Telefon numarası başında 0 olmadan 10 haneli olmalı (örnek: 5551234567).'),
    password: z
      .string()
      .min(6, 'Şifre en az 6 karakter olmalı.')
      .max(128, 'Şifre en fazla 128 karakter olabilir.'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Şifreler eşleşmiyor.',
    path: ['confirmPassword'],
  })
export type RegisterInput = z.infer<typeof registerSchema>

export const otpSchema = z.object({
  otpCode: z.string().regex(/^\d{6}$/, '6 haneli doğrulama kodunu girin.'),
})
export type OtpInput = z.infer<typeof otpSchema>

// FormField ile birlikte kullanılır: hata varsa input'u geçersiz işaretler ve hata metnine bağlar.
export function fieldA11y(id: string, error: string | undefined) {
  return {
    id,
    'aria-invalid': error ? true : undefined,
    'aria-describedby': error ? `${id}-error` : undefined,
  }
}

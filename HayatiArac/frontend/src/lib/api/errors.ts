// Backend tüm hataları RFC 7807 ProblemDetails olarak döner (bkz. ResultExtensions.ToProblem,
// ExceptionHandlingMiddleware). Validation hatalarında alan bazlı "errors" da gelir.
export type ProblemDetails = {
  title?: string
  status?: number
  detail?: string
  code?: string
  errors?: Record<string, string[]>
}

const FALLBACK_MESSAGES: Record<number, string> = {
  0: 'Sunucuya ulaşılamadı. İnternet bağlantınızı kontrol edin.',
  400: 'Gönderilen bilgiler geçersiz.',
  401: 'Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.',
  403: 'Bu işlem için yetkiniz yok.',
  404: 'Kayıt bulunamadı.',
  409: 'İşlem mevcut kayıtlarla çakışıyor.',
  429: 'Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin.',
}

export class ApiError extends Error {
  readonly status: number
  readonly code: string | undefined
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.detail ?? FALLBACK_MESSAGES[status] ?? 'Beklenmeyen bir hata oluştu.')
    this.name = 'ApiError'
    this.status = status
    this.code = problem?.code
    this.fieldErrors = problem?.errors ?? {}
  }
}

export async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | null = null
  if (response.headers.get('content-type')?.includes('json')) {
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Gövde boş veya bozuk: varsayılan mesaj kullanılır.
    }
  }
  return new ApiError(response.status, problem)
}

import { z } from 'zod'

// Kayıttan sonra doğrulama sayfası userId'yi sayfa yenilense de bilmeli.
// userId bir sır değildir (token değil); yine de yalnızca sekme ömrü boyunca
// sessionStorage'da tutulur ve doğrulama bitince silinir.
const STORAGE_KEY = 'hayatiarac.pendingVerification'

const pendingSchema = z.object({
  userId: z.uuid(),
  email: z.string(),
})
export type PendingVerification = z.infer<typeof pendingSchema>

export function savePendingVerification(value: PendingVerification) {
  try {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(value))
  } catch {
    // Depolama kapalı olabilir (gizli mod vb.); doğrulama sayfası bunu ayrıca ele alır.
  }
}

export function readPendingVerification(): PendingVerification | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const parsed = pendingSchema.safeParse(JSON.parse(raw))
    return parsed.success ? parsed.data : null
  } catch {
    return null
  }
}

export function clearPendingVerification() {
  try {
    sessionStorage.removeItem(STORAGE_KEY)
  } catch {
    // Yok sayılır.
  }
}

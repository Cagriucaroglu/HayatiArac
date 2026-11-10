// Kullanıcı kaynaklı URL'ler (ilan görselleri vb.) yalnızca mutlak https adresiyse render edilir.
// javascript:, data:, http: ve göreli adresler null döner; çağıran taraf bu durumda hiçbir şey göstermez.
export function safeUrl(value: string | null | undefined): string | null {
  if (!value) return null

  let url: URL
  try {
    url = new URL(value)
  } catch {
    return null
  }

  return url.protocol === 'https:' ? url.href : null
}

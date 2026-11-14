// Login sonrası "?redirect=" ile dönülecek adres yalnızca uygulama içi bir path olabilir.
// "https://evil.com", "//evil.com", "/\evil.com" gibi değerler open redirect'e yol açar; fallback döner.
export function safeRedirect(value: string | null | undefined, fallback = '/'): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) {
    return fallback
  }

  try {
    const url = new URL(value, window.location.origin)
    if (url.origin !== window.location.origin) return fallback
    return `${url.pathname}${url.search}${url.hash}`
  } catch {
    return fallback
  }
}

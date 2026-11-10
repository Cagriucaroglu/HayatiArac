import { describe, expect, it } from 'vitest'
import { safeUrl } from './safeUrl'

describe('safeUrl', () => {
  it('https adresini kabul eder', () => {
    expect(safeUrl('https://cdn.example.com/a.jpg')).toBe('https://cdn.example.com/a.jpg')
  })

  it.each([
    'javascript:alert(1)',
    'JaVaScRiPt:alert(1)',
    'data:image/png;base64,AAAA',
    'http://example.com/a.jpg',
    '/relative/path.jpg',
    '//evil.example.com/a.jpg',
    'not a url',
    '',
  ])('%s adresini reddeder', (value) => {
    expect(safeUrl(value)).toBeNull()
  })

  it('null ve undefined için null döner', () => {
    expect(safeUrl(null)).toBeNull()
    expect(safeUrl(undefined)).toBeNull()
  })
})

import { describe, expect, it } from 'vitest'
import { safeRedirect } from './safeRedirect'

describe('safeRedirect', () => {
  it.each(['/profil', '/ilan/123?tab=foto', '/favoriler#liste'])('%s uygulama içi path olarak kabul edilir', (value) => {
    expect(safeRedirect(value)).toBe(value)
  })

  it.each([
    'https://evil.example.com',
    '//evil.example.com',
    '/\\evil.example.com',
    '/\t/evil.example.com',
    'javascript:alert(1)',
    'profil',
    '',
  ])('%s reddedilir ve fallback döner', (value) => {
    expect(safeRedirect(value)).toBe('/')
  })

  it('null için fallback döner', () => {
    expect(safeRedirect(null, '/giris')).toBe('/giris')
  })
})

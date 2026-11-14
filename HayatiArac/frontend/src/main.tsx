import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router'
import './index.css'
import { AppProviders } from './app/providers'
import { router } from './app/router'
import { initSession } from './lib/auth/session'

// Render'dan önce başlatılır: HttpOnly cookie geçerliyse oturum sessizce geri gelir.
void initSession()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>
  </StrictMode>,
)

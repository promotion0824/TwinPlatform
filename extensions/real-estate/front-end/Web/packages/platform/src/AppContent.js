import { useEffect } from 'react'
import { initApi } from '@willow/ui'
import Site from './views/Site'
import { authService } from '@willow/common'

export default function AppContent() {
  useEffect(() => {
    initApi({
      onError(err, url) {
        if (
          err?.response?.status === 401 &&
          url?.startsWith('/') &&
          url !== '/api/me' &&
          !window.location.pathname.startsWith('/account') &&
          window.location.pathname !== '/public/3d.html'
        ) {
          window.location = authService.getLoginPath()
          return true
        }

        return false
      },
    })
  }, [])

  return <Site />
}

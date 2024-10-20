import { useEffect } from 'react'
import { authService } from '@willow/common'

export default function SilentRenewCallback({
  app,
}: {
  app: 'mobile' | 'platform'
}) {
  useEffect(() => {
    authService.readSilentRenewResponse(app)
  }, [app])

  return null
}

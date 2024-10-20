import { authService } from '@willow/common'
import { useEffect } from 'react'

export default function Login({ useConfig }: { useConfig: () => unknown }) {
  const config = useConfig()

  useEffect(() => {
    authService.login(config)
  }, [config])

  return null
}

import { AuthCallbackUI, qs } from '@willow/common'
import { setAuthConfigToLocalStorage } from '@willow/common/authService'
import { setApiGlobalPrefix, useAnalytics, useApi } from '@willow/ui'
import { useEffect, useState } from 'react'
import { useHistory } from 'react-router'

const FALLBACK_EXPIRY_IN_SECONDS = 3000 // 50 minutes

export default function Auth0Callback() {
  const analytics = useAnalytics()
  const api = useApi()
  const history = useHistory()

  const [hasError, setHasError] = useState(false)

  async function signIn() {
    const authorizationCode = qs.get('code')
    const token = qs.get('token')
    const regionCode = qs.get('regionCode')

    setApiGlobalPrefix(regionCode)

    try {
      const { userId } = await api.post(
        '/api/me/signin',
        authorizationCode != null
          ? {
              authorizationCode,
              redirectUri: window.location.href,
            }
          : {
              token,
            }
      )

      // These logins come from the Admin Portal, so a fallback expiresIn value is set for them.
      setAuthConfigToLocalStorage(userId, FALLBACK_EXPIRY_IN_SECONDS)

      analytics.reset()
      history.replace(qs.get('redirect') ?? '/')
    } catch (err) {
      console.error(err)
      setHasError(true)
    }
  }

  useEffect(() => {
    signIn()
  }, [])

  return (
    <>
      <AuthCallbackUI hasError={hasError} />
    </>
  )
}

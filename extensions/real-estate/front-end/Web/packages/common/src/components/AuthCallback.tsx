import { useEffect, useState } from 'react'
import { authService } from '@willow/common'
import AuthCallbackUI from './AuthCallbackUI'

export default function AuthCallback({
  app,
  resetPassword,
}: {
  app: 'platform' | 'mobile'
  resetPassword: boolean
}) {
  const [state, setState] = useState<'loading' | 'success' | 'error'>('loading')

  useEffect(() => {
    async function signIn() {
      try {
        if (resetPassword) {
          await authService.readResetPasswordResponse(app)
        } else {
          await authService.readSigninResponse(app)
        }
        // A successful operation will redirect the user away from this
        // component, so we don't really need a success state.
        setState('success')
      } catch (err) {
        console.error(err)
        setState('error')
      }
    }
    signIn()
  }, [])

  return <AuthCallbackUI hasError={state === 'error'} />
}

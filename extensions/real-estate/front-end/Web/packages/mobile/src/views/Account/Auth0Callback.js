import { qs, WillowLogoWhite } from '@willow/common'
import {
  Loader,
  Message,
  setApiGlobalPrefix,
  Spacing,
  useAnalytics,
  useApi,
} from '@willow/mobile-ui'
import { useEffect, useState } from 'react'
import { useHistory } from 'react-router'
import { Link } from 'react-router-dom'
import styles from './Auth0Callback.css'

export default function Auth0Callback() {
  const analytics = useAnalytics()
  const history = useHistory()
  const api = useApi()

  const [hasError, setHasError] = useState(false)

  async function signIn() {
    const authorizationCode = qs.get('code')
    const token = qs.get('token')
    const regionCode = qs.get('regionCode')

    setApiGlobalPrefix(regionCode)

    try {
      await api.post(
        '/api/signin',
        authorizationCode != null
          ? {
              authorizationCode,
              redirectUri: window.location.href,
            }
          : {
              token,
            }
      )

      analytics.reset()
      history.replace(qs.get('redirect') ?? '/')
    } catch (err) {
      setHasError(true)
    }
  }

  useEffect(() => {
    signIn()
  }, [])

  return (
    <div className={styles.auth0Callback}>
      <Link to={hasError ? '/' : ''} className={styles.willow}>
        <WillowLogoWhite height={32} />
      </Link>
      {hasError ? (
        <Message icon="error">
          <Spacing size="small">
            <p>An error has occurred trying to sign you in.</p>
            <p>Please contact your site administrator.</p>
          </Spacing>
        </Message>
      ) : (
        <Loader size="extraLarge" />
      )}
    </div>
  )
}

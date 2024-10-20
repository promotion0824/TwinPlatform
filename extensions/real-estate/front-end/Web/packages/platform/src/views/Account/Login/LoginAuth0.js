import { useLocation } from 'react-router'
import { WebAuth } from 'auth0-js'
import {
  cookie,
  useConfig,
  AccountButton,
  AccountInput,
  AccountLinkButton,
  AccountPasswordInput,
  Flex,
  Form,
  ValidationError,
  Text,
} from '@willow/ui'
import { qs } from '@willow/common'
import { useTranslation } from 'react-i18next'
import CountrySelect from '../CountrySelect/CountrySelect'
import styles from './LoginAuth0.css'

export default function LoginAuth0() {
  const config = useConfig()
  const location = useLocation()
  const { t } = useTranslation()

  function handleSubmit(form) {
    const country = cookie.get('api')

    return new Promise((resolve, reject) => {
      const auth0 = new WebAuth({
        domain: config[`auth0Domain-${country}`],
        clientID: config[`auth0ClientID-${country}`],
        audience: config[`auth0Audience-${country}`],
      })

      auth0.login(
        {
          realm: 'Username-Password-Authentication',
          email: form.data.email,
          password: form.data.password,
          redirectUri: qs.createUrl(
            `${window.location.origin}/account/auth0callback`,
            {
              redirect: qs.get('redirect'),
              regionCode: country,
            }
          ),
          responseType: 'code',
        },
        (err) => {
          if (err.code !== 'request_error') {
            if (err.code === 'invalid_request') {
              reject(new ValidationError('Email and password must be given.'))
              return
            }
            if (err.code === 'access_denied') {
              reject(
                new ValidationError({
                  message: t('plainText.wrongEmailPass'),
                  description:
                    'Have you selected the right region to login to?',
                })
              )
              return
            }

            reject(new ValidationError(err.error_description))
            return
          }

          reject()
        }
      )
    })
  }

  let message = 'Welcome back,'
  if (location.state?.create) message = 'Your password has been created,'
  if (location.state?.reset) message = 'Your password has been reset,'

  return (
    <Form debounce={false} onSubmit={handleSubmit}>
      {(form) => (
        <Flex size="extraLarge">
          <Text size="extraLarge" color="white">
            <div>{message}</div>
            <div>sign in to your account:</div>
          </Text>
          <Flex>
            <CountrySelect />
            <AccountInput
              name="email"
              icon="email"
              type="email"
              placeholder={t('placeholder.enterEmail')}
              error={form.errors.length > 0}
              autoComplete="username"
              className={styles.input}
            />
            <AccountPasswordInput
              name="password"
              placeholder={t('placeholder.enterPassword')}
              error={form.errors.length > 0}
              autoComplete="current-password"
              passwordInputClassName={styles.input}
            />
          </Flex>
          <Flex horizontal fill="content">
            <AccountButton type="submit">Sign in</AccountButton>
            <Flex align="top right">
              <AccountLinkButton to="/account/forgot-password">
                Forgot password?
              </AccountLinkButton>
            </Flex>
          </Flex>
        </Flex>
      )}
    </Form>
  )
}

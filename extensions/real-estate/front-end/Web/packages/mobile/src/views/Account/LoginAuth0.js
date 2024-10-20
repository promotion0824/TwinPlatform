import { useLocation } from 'react-router'
import { WebAuth } from 'auth0-js'
import {
  cookie,
  useConfig,
  Button,
  Form,
  Input,
  Passwordbox,
  Spacing,
  ValidationError,
} from '@willow/mobile-ui'
import { qs } from '@willow/common'
import Group from './Group/Group'
import CountrySelect from './CountrySelect/CountrySelect'
import styles from './AccountLayout/AccountLayout.css'

export default function LoginAuth0() {
  const location = useLocation()
  const config = useConfig()

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
          email: form.value.email,
          password: form.value.password,
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
                  message: 'Wrong email or password',
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
    <Form onSubmit={handleSubmit}>
      {(form) => (
        <Spacing size="extraLarge">
          <div className={styles.text}>
            <div>{message}</div>
            <div>sign in to your account:</div>
          </div>
          <Group>
            <CountrySelect className={styles.select} />
            <Input
              name="email"
              icon="email"
              placeholder="enter your email address"
              error={form.errors.length > 0}
              autoComplete="username"
              className={styles.input}
              iconClassName={styles.icon}
            />
            <Passwordbox
              name="password"
              size="large"
              placeholder="enter your password"
              error={form.errors.length > 0}
              autoComplete="current-password"
              className={styles.input}
              iconClassName={styles.icon}
            />
          </Group>
          <Spacing horizontal responsive type="header">
            <div>
              <Button
                type="submit"
                color="white"
                size="large"
                disabled={false}
                className={styles.submitButton}
              >
                Sign in
              </Button>
            </div>
            <div>
              <Button to="/account/forgot-password" className={styles.link}>
                Forgot password?
              </Button>
            </div>
          </Spacing>
        </Spacing>
      )}
    </Form>
  )
}

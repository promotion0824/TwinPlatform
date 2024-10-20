import { useState } from 'react'
import { useLocation, Redirect } from 'react-router'
import {
  setApiGlobalPrefix,
  useApi,
  Button,
  Fetch,
  Form,
  Link,
  Message,
  Passwordbox,
  Spacing,
  Text,
} from '@willow/mobile-ui'
import { qs } from '@willow/common'
import Validations from './Validations'
import rules from './rules'
import styles from '../AccountLayout/AccountLayout.css'

export default function ResetPassword() {
  const [token] = useState(() => {
    const region = qs.get('region')
    setApiGlobalPrefix(region)

    return qs.get('t')
  })

  const location = useLocation()
  const api = useApi()

  const isResetPassword = location.pathname.endsWith('reset-password')

  const success = () => (
    <Redirect
      to={{
        pathname: '/account/login',
        state: {
          create: !isResetPassword,
          reset: isResetPassword,
        },
      }}
    />
  )

  return (
    <Fetch url={`/api/userResetPasswordTokens/${token}`} error={null}>
      {(response, fetch) => {
        function handleSubmit(form) {
          const method = isResetPassword ? 'put' : 'post'

          const url = `/api/users/${window.encodeURIComponent(
            response.email
          )}/password`

          return api.ajax(method, url, {
            token,
            password: form.value.password,
          })
        }

        if (fetch.error) {
          return (
            <div>
              <Message icon="error">
                {fetch.error.status === 401 ? (
                  <Spacing size="tiny">
                    {isResetPassword ? (
                      <>
                        <Text size="large">
                          Your reset password request has expired.
                        </Text>
                        <Text>
                          <span>Please request another password reset </span>
                          <Link to="/account/forgot-password">here</Link>
                          <span>.</span>
                        </Text>
                      </>
                    ) : (
                      <>
                        <Text size="large">
                          Your account activation has expired.
                        </Text>
                        <Text>
                          Please contact your administrator to request a new
                          activation email.
                        </Text>
                      </>
                    )}
                  </Spacing>
                ) : (
                  <Redirect to="/account/login" />
                )}
              </Message>
              {!isResetPassword && (
                <Spacing align="right">
                  <Button to="/account/login" className={styles.link}>
                    OK
                  </Button>
                </Spacing>
              )}
            </div>
          )
        }

        return (
          <Form showSubmitted={false} success={success} onSubmit={handleSubmit}>
            {(form) => {
              const password = form.value.password ?? ''
              const isValid = rules.every((rule) => rule.isValid(password))

              return (
                <Spacing size="extraLarge">
                  <div className={styles.text}>
                    {isResetPassword
                      ? `Reset the password for your ${response.email} account.`
                      : `Create the password for your ${response.email} account.`}
                  </div>
                  <Spacing horizontal type="header">
                    <Spacing size="extraLarge" width="100%">
                      <Passwordbox
                        name="password"
                        size="large"
                        placeholder="enter your new password"
                        error={form.errors.length > 0}
                        autoComplete="current-password"
                        maxLength={64}
                        className={styles.input}
                        iconClassName={styles.icon}
                      />
                      <Spacing horizontal type="header">
                        <div>
                          <Button
                            type="submit"
                            color="white"
                            size="large"
                            disabled={!isValid}
                            className={styles.submitButton}
                          >
                            {isResetPassword
                              ? 'Reset password'
                              : 'Create password'}
                          </Button>
                        </div>
                      </Spacing>
                    </Spacing>
                    <div>
                      <Validations value={password} />
                    </div>
                  </Spacing>
                </Spacing>
              )
            }}
          </Form>
        )
      }}
    </Fetch>
  )
}

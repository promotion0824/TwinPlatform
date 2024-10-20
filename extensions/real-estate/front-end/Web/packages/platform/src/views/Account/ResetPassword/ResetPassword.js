import { useState } from 'react'
import { useLocation, Redirect } from 'react-router'
import {
  setApiGlobalPrefix,
  AccountButton,
  AccountLinkButton,
  AccountPasswordInput,
  Fetch,
  Flex,
  Form,
  Link,
  Message,
  Text,
} from '@willow/ui'
import { qs } from '@willow/common'
import { useTranslation } from 'react-i18next'
import Validations from './Validations'
import rules from './rules'
import styles from './ResetPassword.css'

export default function ResetPassword() {
  const location = useLocation()
  const { t } = useTranslation()

  const [token] = useState(() => {
    const region = qs.get('region')
    setApiGlobalPrefix(region)

    return qs.get('t')
  })

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
    <Fetch
      url={
        isResetPassword
          ? `/api/resetPasswordTokens/${token}`
          : `/api/initializeUserTokens/${token}`
      }
      error={null}
    >
      {(response, fetch) => {
        function handleSubmit(form) {
          const method = isResetPassword ? 'put' : 'post'

          const url = isResetPassword
            ? `/api/users/${window.encodeURIComponent(response.email)}/password`
            : `/api/users/${window.encodeURIComponent(
                response.email
              )}/initialize`

          return form.api.ajax(url, {
            method,
            body: {
              token,
              password: form.data.password,
            },
          })
        }

        if (fetch.error) {
          return (
            <Flex size="large">
              <Message icon="error">
                {fetch.error.status === 401 ? (
                  <Flex size="tiny">
                    {isResetPassword ? (
                      <Flex size="tiny">
                        <Text type="message" size="large">
                          {t('plainText.resetPasswordExpired')}
                        </Text>
                        <Text>
                          <span>{t('plainText.requestAnotherPassReset')} </span>
                          <Link to="/account/forgot-password">
                            {t('plainText.here')}
                          </Link>
                          <span>.</span>
                        </Text>
                      </Flex>
                    ) : (
                      <>
                        <Text type="message" size="large">
                          {t('plainText.accountActivateExpired')}
                        </Text>
                        <Text>{t('plainText.contactAdmin')}</Text>
                      </>
                    )}
                  </Flex>
                ) : (
                  <Redirect to="/account/login" />
                )}
              </Message>
              {!isResetPassword && (
                <Flex align="right">
                  <AccountLinkButton to="/account/login">
                    {t('plainText.ok')}
                  </AccountLinkButton>
                </Flex>
              )}
            </Flex>
          )
        }

        return (
          <Form debounce={false} success={success} onSubmit={handleSubmit}>
            {(form) => {
              const password = form.data.password ?? ''
              const isValid = rules.every((rule) => rule.isValid(password))

              return (
                <Flex size="extraLarge">
                  <Text size="extraLarge" color="white">
                    {isResetPassword
                      ? `Reset the password for your ${response.email} account.`
                      : `Create the password for your ${response.email} account.`}
                  </Text>
                  <Flex horizontal>
                    <Flex size="extraLarge">
                      <AccountPasswordInput
                        name="password"
                        size="large"
                        placeholder={t('placeholder.enterNewPassword')}
                        autoComplete="current-password"
                        maxLength={64}
                        className={styles.passwordInput}
                      />
                      <AccountButton type="submit" disabled={!isValid}>
                        {isResetPassword
                          ? t('plainText.resetPassword')
                          : t('plainText.createPassword')}
                      </AccountButton>
                    </Flex>
                    <div>
                      <Validations value={password} />
                    </div>
                  </Flex>
                </Flex>
              )
            }}
          </Form>
        )
      }}
    </Fetch>
  )
}

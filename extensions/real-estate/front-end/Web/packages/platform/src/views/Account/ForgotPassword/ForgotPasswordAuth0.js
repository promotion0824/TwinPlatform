import {
  AccountButton,
  AccountInput,
  AccountLinkButton,
  Flex,
  Form,
  Message,
  Text,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import CountrySelect from '../CountrySelect/CountrySelect'
import styles from './ForgotPassword.css'

export default function ForgotPasswordAuth0() {
  const { t } = useTranslation()
  const success = () => (
    <Flex size="extraLarge">
      <Text size="extraLarge" color="white">
        <div>{t('plainText.enterAccountDetails')}</div>
        <div>{t('plainText.toResetPassword')}:</div>
      </Text>
      <Message icon="ok" className={styles.message}>
        {t('plainText.resetPasswordLink')}
      </Message>
      <Flex align="right">
        <AccountLinkButton to="/account/login">
          {t('plainText.ok')}
        </AccountLinkButton>
      </Flex>
    </Flex>
  )

  function handleSubmit(form) {
    return form.api.post(`/api/users/${form.data.email}/password/reset`)
  }

  return (
    <Form debounce={false} onSubmit={handleSubmit} success={success}>
      {(form) => (
        <Flex size="extraLarge">
          <Text size="extraLarge" color="white">
            <div>{t('plainText.enterAccountDetails')}</div>
            <div>{t('plainText.toResetPassword')}:</div>
          </Text>
          <Flex>
            <CountrySelect />
            <AccountInput
              name="email"
              icon="email"
              placeholder={t('placeholder.enterEmail')}
              error={form.errors.length > 0}
              autoComplete="username"
              className={styles.input}
            />
          </Flex>
          <Flex horizontal fill="content">
            <AccountButton
              type="submit"
              disabled={form.data.email?.length === 0}
            >
              {t('plainText.submit')}
            </AccountButton>
            <Flex horizontal align="top right">
              <AccountLinkButton to="/account/login">
                {t('plainText.cancel')}
              </AccountLinkButton>
            </Flex>
          </Flex>
        </Flex>
      )}
    </Form>
  )
}

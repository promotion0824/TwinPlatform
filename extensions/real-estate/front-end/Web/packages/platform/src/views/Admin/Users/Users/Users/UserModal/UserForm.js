import { useState } from 'react'
import {
  useFetchRefresh,
  useSnackbar,
  useUser,
  DatePicker,
  Fieldset,
  Flex,
  Form,
  Input,
  ModalSubmitButton,
  Select,
  Option,
  Text,
  useLanguage,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import DeleteUserModal from './DeleteUserModal'
import Permissions from './Permissions'
import ResendActivationEmailModal from './ResendActivationEmailModal'
import styles from './UserForm.css'

export default function UserForm({ user, portfoliosResponse, isNewUser }) {
  const currentUser = useUser()
  const fetchRefresh = useFetchRefresh()
  const snackbar = useSnackbar()
  const { t } = useTranslation()
  const { language } = useLanguage()
  const [showResendActivationEmailModal, setShowResendActivationEmailModal] =
    useState(false)
  const [showDeleteUserModal, setShowDeleteUserModal] = useState(false)

  const readOnly = !isNewUser && !user.canEdit

  function handleSubmit(form) {
    const portfolios = portfoliosResponse
      .filter(
        (portfolio) =>
          portfolio.role === 'Admin' ||
          portfolio.sites.some((site) => site.role === 'Admin')
      )
      .map((portfolio) => ({
        ...portfolio,
        sites: portfolio.sites.filter((site) => site.role === 'Admin'),
      }))
      .map((portfolio) => ({
        portfolioId: portfolio.portfolioId,
        portfolioName: portfolio.portfolioName,
        role:
          form.data.portfolioRoles.find(
            (portfolioRole) =>
              portfolioRole.portfolioId === portfolio.portfolioId
          )?.role ?? '',
        sites: portfolio.sites.map((site) => ({
          siteId: site.siteId,
          siteName: site.siteName,
          role:
            form.data.siteRoles.find(
              (siteRole) => siteRole.siteId === site.siteId
            )?.role ?? '',
        })),
      }))
      .map((portfolio) => ({
        ...portfolio,
        role: form.data.isCustomerAdmin ? 'Admin' : portfolio.role,
      }))
      .map((portfolio) => ({
        ...portfolio,
        sites: portfolio.sites.map((site) => ({
          ...site,
          role: portfolio.role === 'Admin' ? 'Admin' : site.role,
        })),
      }))
      .map((portfolio) => ({
        ...portfolio,
        sites: portfolio.sites.map((site) => ({
          ...site,
          role:
            portfolio.role === 'Viewer' && site.role === ''
              ? 'Viewer'
              : site.role,
        })),
      }))
      .filter(
        (portfolio) => portfolio.role !== '' || portfolio.sites.length > 0
      )

    if (!isNewUser) {
      return form.api.put(
        `/api/management/customers/${currentUser.customer.id}/users/${user.id}`,
        {
          firstName: form.data.firstName,
          lastName: form.data.lastName,
          email: form.data.email,
          contactNumber: form.data.contactNumber,
          company: form.data.company,
          isCustomerAdmin: form.data.isCustomerAdmin,
          portfolios,
        }
      )
    }

    return form.api.post(
      `/api/management/customers/${currentUser.customer.id}/users`,
      {
        firstName: form.data.firstName,
        lastName: form.data.lastName,
        email: form.data.email,
        contactNumber: form.data.contactNumber,
        company: form.data.company,
        useB2C: true,
        isCustomerAdmin: form.data.isCustomerAdmin,
        portfolios,
      },
      {
        headers: { language },
      }
    )
  }

  function handleSubmitted(form) {
    if (form.response?.message != null) {
      snackbar.show(form.response.message, {
        icon: 'ok',
      })
    }

    form.modal.close()

    fetchRefresh('users')
  }

  return (
    <>
      <Form
        defaultValue={user}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        {(form) => (
          <Flex fill="header">
            <Flex>
              <Flex horizontal fill="content">
                <Flex padding="extraLarge 0 extraLarge extraLarge">
                  <div className={styles.user}>
                    {form.data.firstName.length > 0 &&
                      form.data.lastName.length > 0 && (
                        <Text size="extraLarge" color="white">
                          {form.data.firstName[0]?.toUpperCase()}
                          {form.data.lastName[0]?.toUpperCase()}
                        </Text>
                      )}
                  </div>
                </Flex>
                <Fieldset legend={t('plainText.generalInfo')}>
                  <Input
                    name="firstName"
                    label={t('labels.firstName')}
                    required
                    readOnly={readOnly}
                  />
                  <Input
                    name="lastName"
                    label={t('labels.lastName')}
                    required
                    readOnly={readOnly}
                  />
                  <Input
                    name="email"
                    label={t('labels.emailAddress')}
                    readOnly={readOnly || !isNewUser}
                    required
                  />
                  <Input
                    name="contactNumber"
                    label={t('labels.contact')}
                    required
                    readOnly={readOnly}
                  />
                  <Input
                    name="company"
                    label={t('labels.company')}
                    required
                    readOnly={readOnly}
                  />
                  {!isNewUser ? (
                    <DatePicker
                      label={t('labels.userSince')}
                      type="date-time"
                      value={user.createdDate}
                      readOnly
                    />
                  ) : (
                    <div />
                  )}
                </Fieldset>
              </Flex>
              {currentUser.isCustomerAdmin && (
                <Fieldset legend={t('plainText.permissions')}>
                  <Select
                    name="isCustomerAdmin"
                    label={t('labels.role')}
                    unselectable
                    required
                    width="medium"
                    readOnly={readOnly}
                  >
                    <Option value>{t('labels.superUser')}</Option>
                    <Option value={false}>{t('plainText.user')}</Option>
                  </Select>
                </Fieldset>
              )}
              {!form.data.isCustomerAdmin && (
                <Permissions
                  portfoliosResponse={portfoliosResponse}
                  readOnly={readOnly}
                />
              )}
              {(!isNewUser || user.status === 'pending') && (
                <Flex align="center" size="large" padding="extraLarge">
                  {user.status === 'pending' && (
                    <Button
                      onClick={() => setShowResendActivationEmailModal(true)}
                    >
                      {t('headers.resendActivationEmail')}
                    </Button>
                  )}
                  {!isNewUser && currentUser.isCustomerAdmin && !readOnly && (
                    <Button
                      kind="negative"
                      disabled={currentUser.id === user.id}
                      onClick={() => setShowDeleteUserModal(true)}
                    >
                      {t('headers.deleteUser')}
                    </Button>
                  )}
                </Flex>
              )}
            </Flex>
            <ModalSubmitButton showSubmitButton={!readOnly}>
              {t('plainText.save')}
            </ModalSubmitButton>
          </Flex>
        )}
      </Form>
      {showResendActivationEmailModal && (
        <ResendActivationEmailModal
          user={user}
          onClose={() => setShowResendActivationEmailModal(false)}
        />
      )}
      {showDeleteUserModal && (
        <DeleteUserModal
          user={user}
          onClose={() => setShowDeleteUserModal(false)}
        />
      )}
    </>
  )
}

import { useUser, Fetch, Modal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import UserForm from './UserForm'

export default function UserModal({ user, onClose }) {
  const currentUser = useUser()
  const { t } = useTranslation()

  const isNewUser = user.id == null

  return (
    <Modal
      header={isNewUser ? t('plainText.addUser') : t('headers.editUser')}
      size="medium"
      onClose={onClose}
    >
      <Fetch
        url={[
          !isNewUser
            ? `/api/management/customers/${currentUser.customer.id}/users/${user.id}`
            : undefined,
          '/api/management/managedPortfolios',
        ]}
      >
        {([userResponse, portfoliosResponse]) => {
          const nextUser = {
            ...(userResponse ?? user),
            portfolioRoles: (userResponse?.portfolios ?? [])
              .filter((portfolio) => portfolio.role !== '')
              .map((portfolio) => ({
                portfolioId: portfolio.portfolioId,
                role: portfolio.role,
              })),
            siteRoles: (userResponse?.portfolios ?? [])
              .flatMap((portfolio) => portfolio.sites)
              .map((site) => ({
                siteId: site.siteId,
                role: site.role,
              })),
          }

          return (
            <UserForm
              user={nextUser}
              portfoliosResponse={portfoliosResponse}
              isNewUser={isNewUser}
            />
          )
        }}
      </Fetch>
    </Modal>
  )
}

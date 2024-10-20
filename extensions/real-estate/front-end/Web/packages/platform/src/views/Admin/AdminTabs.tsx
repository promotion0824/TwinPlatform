import { titleCase } from '@willow/common'
import isWillowUser from '@willow/common/utils/isWillowUser'
import {
  caseInsensitiveEquals,
  NavTab,
  NavTabs,
  useFeatureFlag,
  useUser,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import routes from '../../routes'

export default function AdminTabs() {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const featureFlags = useFeatureFlag()
  const { isCustomerAdmin, email, customer, showAdminMenu } = useUser()

  const showModelsOfInterest = isCustomerAdmin
  const willowUser = isWillowUser(email)
  const isDemo = caseInsensitiveEquals(
    customer?.name,
    internalUsedDemoClientName
  )

  return (
    <NavTabs
      tabs={[
        ...(showAdminMenu
          ? [
              <NavTab to={routes.admin} value="portfolios">
                {t('headers.portfolios')}
              </NavTab>,
              <NavTab
                include={[routes.admin_requestors, routes.admin_workgroups]}
                to={routes.admin_users}
                value="users"
              >
                {t('labels.users')}
              </NavTab>,
            ]
          : []),
        ...(showModelsOfInterest
          ? [
              <NavTab
                to={routes.admin_models_of_interest}
                value="modelsOfInterest"
              >
                {t('plainText.modelsOfInterest')}
              </NavTab>,
            ]
          : []),
        ...(featureFlags.hasFeatureToggle('isNotificationEnabled')
          ? [
              <NavTab
                to={routes.admin_notification_settings}
                value="notificationSettings"
              >
                {titleCase({
                  text: t('headers.notificationSettings'),
                  language,
                })}
              </NavTab>,
            ]
          : []),
        /* the following tab is only relevant for Willow internal user for demo purpose, no need to translate */
        ...(willowUser && isDemo
          ? [
              <NavTab to="/admin/sandbox" value="sandbox">
                Launch Sandbox
              </NavTab>,
            ]
          : []),
      ]}
      variant="pills"
    />
  )
}

/**
 * DDK Investments is a customer that is used for demo purposes,
 * we add some sandbox links only for this customer.
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/96817
 */
const internalUsedDemoClientName = 'DDK Investments'

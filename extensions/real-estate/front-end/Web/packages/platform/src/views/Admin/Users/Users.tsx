import { titleCase } from '@willow/common'
import { DocumentTitle, Flex, Tab, Tabs, useFeatureFlag } from '@willow/ui'
import { Button, EmptyState, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import SplitHeaderPanel from '../../Layout/Layout/SplitHeaderPanel'
import AdminTabs from '../AdminTabs'
import Requestors from './Requestors/Requestors'
import Users from './Users/Users'
import Workgroups from './Workgroups/Workgroups'

/**
 * UsersComponent displays tabs in Admin page to manage user accounts
 */
export default function UsersComponent() {
  const featureFlags = useFeatureFlag()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <>
      <DocumentTitle scopes={[t('labels.users'), t('headers.admin')]} />

      <SplitHeaderPanel leftElement={<AdminTabs />} />

      {featureFlags.hasFeatureToggle('userManagementLink') ? (
        <EmptyState
          description={t('plainText.userAdminHasMovedDetails')}
          icon="info"
          primaryActions={
            <Button
              href="/authrz-web"
              suffix={<Icon icon="open_in_new" />}
              target="_blank"
            >
              {titleCase({
                language,
                text: t('labels.openUserManagement'),
              })}
            </Button>
          }
          title={t('plainText.userAdminHasMoved')}
        />
      ) : (
        <Flex padding="small 0 0 0">
          <Tabs $borderWidth="1px 0 0 0">
            <Tab
              header={t('labels.users')}
              to="/admin/users"
              data-testid="users-tab"
            >
              <Users />
            </Tab>
            <Tab
              header={t('headers.requestors')}
              to="/admin/requestors"
              data-testid="requestors-tab"
            >
              <Requestors />
            </Tab>
            <Tab
              header={t('headers.workgroups')}
              to="/admin/workgroups"
              data-testid="workgroups-tab"
            >
              <Workgroups />
            </Tab>
          </Tabs>
        </Flex>
      )}
    </>
  )
}

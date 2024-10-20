import _ from 'lodash'
import {
  Fetch,
  Flex,
  Header,
  Input,
  NotFound,
  Select,
  Option,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useWorkgroups } from './WorkgroupsContext'
import WorkgroupsTable from './WorkgroupsTable'

export default function WorkgroupsContent() {
  const workgroups = useWorkgroups()
  const { t } = useTranslation()

  return (
    <Flex fill="content">
      <Header>
        <Select
          placeholder={t('placeholder.sites')}
          width="medium"
          value={workgroups.selectedSite}
          onChange={(nextSite) => workgroups.setSelectedSite(nextSite)}
        >
          {workgroups.sites.map((site) => (
            <Option key={site.siteId} value={site}>
              {site.siteName}
            </Option>
          ))}
        </Select>
        <Input
          icon="search"
          placeholder={t('labels.search')}
          value={workgroups.search}
          onChange={workgroups.setSearch}
        />
      </Header>
      {workgroups.selectedSite != null && (
        <Fetch
          name="workgroups"
          url={[
            `/api/management/sites/${workgroups.selectedSite.siteId}/workgroups`,
            '/api/me/persons',
          ]}
        >
          {([workgroupsResponse, usersResponse]) => (
            <WorkgroupsTable
              workgroupsResponse={workgroupsResponse}
              usersResponse={_(usersResponse)
                .filter(
                  (user) =>
                    user.type === 'customerUser' && user.status === 'active'
                )
                .orderBy((user) =>
                  `${user.firstName} ${user.lastName}`.toLowerCase()
                )
                .value()}
              search={workgroups.search}
            />
          )}
        </Fetch>
      )}
      {workgroups.selectedSite == null && (
        <NotFound>{t('plainText.selectASite')}</NotFound>
      )}
    </Flex>
  )
}

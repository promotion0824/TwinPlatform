import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Flex, Table, Head, Body, Row, Cell } from '@willow/ui'
import { useWorkgroups } from './WorkgroupsContext'

export default function WorkgroupsTable({
  workgroupsResponse,
  usersResponse,
  search,
}) {
  const workgroupsContext = useWorkgroups()
  const { t } = useTranslation()

  const filteredWorkgroups = workgroupsResponse
    .map((workgroup) => ({
      ...workgroup,
      users: _(workgroup.memberIds)
        .map(
          (memberId) =>
            usersResponse.find((user) => user.id === memberId) ?? {
              id: memberId,
              firstName: 'Unknown',
              lastName: 'User',
            }
        )
        .orderBy((user) => `${user.firstName} ${user.lastName}`.toLowerCase())
        .value(),
    }))
    .filter(
      (workgroup) =>
        workgroup.name.toLowerCase().includes(search.toLowerCase()) ||
        workgroup.users.some((user) =>
          `${user.firstName} ${user.lastName}`
            .toLowerCase()
            .includes(search.toLowerCase())
        )
    )

  return (
    <Table
      items={filteredWorkgroups}
      notFound={t('plainText.noWorkgroupsFound')}
    >
      {(workgroups) => (
        <>
          <Head>
            <Row>
              <Cell sort="name">{t('labels.name')}</Cell>
              <Cell width="1fr">{t('labels.users')}</Cell>
            </Row>
          </Head>
          <Body>
            {workgroups.map((workgroup) => (
              <Row
                key={workgroup.id}
                selected={
                  workgroup.id === workgroupsContext.selectedWorkgroup?.id
                }
                onClick={() =>
                  workgroupsContext.setSelectedWorkgroup(workgroup)
                }
              >
                <Cell align="top">{workgroup.name}</Cell>
                <Cell type="fill">
                  <Flex size="tiny" padding="large 0">
                    {workgroup.users.map((user) => (
                      <div key={user.id}>
                        {user.firstName === 'Unknown' &&
                        user.lastName === 'User'
                          ? t('plainText.unknownUser')
                          : `${user.firstName || ''} ${user.lastName || ''}`}
                      </div>
                    ))}
                  </Flex>
                </Cell>
              </Row>
            ))}
          </Body>
        </>
      )}
    </Table>
  )
}

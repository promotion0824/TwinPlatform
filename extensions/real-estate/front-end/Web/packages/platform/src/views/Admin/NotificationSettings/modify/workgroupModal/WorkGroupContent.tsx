import { FullSizeLoader, titleCase } from '@willow/common'
import { useQueryClient } from 'react-query'
import { NotFound } from '@willow/ui'
import _ from 'lodash'
import {
  Button,
  Group,
  Stack,
  Icon,
  Badge,
  useTheme,
  Link,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import WorkgroupList, { UserText } from '../WorkgroupList'
import { useNotificationSettingsContext } from '../../NotificationSettingsContext'

const WorkgroupContent = ({ searchText }: { searchText?: string }) => {
  const {
    workgroups,
    tempSelectedWorkgroupIds,
    onTempWorkgroupsChange,
    queryStatus,
  } = useNotificationSettingsContext()

  const filteredWorkgroups =
    workgroups?.filter(
      (workgroup) =>
        !searchText ||
        workgroup.name.toLowerCase().includes(searchText?.toLowerCase())
    ) ?? []

  const totalUsers = filteredWorkgroups?.reduce(
    (sum, item) => sum + (item?.memberIds?.length || 0),
    0
  )

  // if all filtered workgroups are selected, return true
  const allFilteredSelected = filteredWorkgroups
    ?.map((item) => item.id)
    .every((id) => tempSelectedWorkgroupIds.includes(id))

  const theme = useTheme()
  const queryClient = useQueryClient()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Group css={{ overflowY: 'auto', display: 'block' }} w="100%" h="380px">
      {queryStatus === 'loading' ? (
        <FullSizeLoader />
      ) : queryStatus === 'success' && filteredWorkgroups?.length > 0 ? (
        <Group w="100%">
          <Stack
            css={{
              alignSelf: 'stretch',
              marginLeft: '20px',
              flex: '1 1 0',
              ...theme.font.heading.group,
              color: theme.color.neutral.fg.muted,
              justifyContent: 'center',
            }}
          >
            <span css={{ display: 'flex', gap: theme.spacing.s4 }}>
              <span>{_.upperCase(t('headers.workgroups'))}</span>
              <Badge color="gray">{filteredWorkgroups?.length}</Badge>
            </span>
          </Stack>
          <Stack
            css={{
              width: '50px',
              alignSelf: 'stretch',
              justifyContent: 'center',
            }}
          >
            <UserText>
              <span>{totalUsers}</span>
              <span>{titleCase({ text: t('labels.users'), language })}</span>
            </UserText>
          </Stack>
          <Stack
            w="100px"
            mr="8px"
            css={{
              alignSelf: 'stretch',
              justifyContent: 'center',
            }}
          >
            <Button
              w="100%"
              kind="secondary"
              prefix={<Icon icon={allFilteredSelected ? 'remove' : 'add'} />}
              onClick={() =>
                onTempWorkgroupsChange(
                  allFilteredSelected
                    ? []
                    : filteredWorkgroups?.map((item) => item.id)
                )
              }
              css={{
                alignSelf: 'stretch',
                justifyContent: 'center',
              }}
            >
              {titleCase({
                text: allFilteredSelected
                  ? t('plainText.removeAll')
                  : t('plainText.addAll'),
                language,
              })}
            </Button>
          </Stack>
          <WorkgroupList
            workgroups={filteredWorkgroups}
            selectedIds={tempSelectedWorkgroupIds}
            isModal
            onSelectIds={onTempWorkgroupsChange}
          />
        </Group>
      ) : (
        [
          {
            errorText: t('plainText.noWorkgroups'),
            linkText: t('plainText.goToUserManagement'),
            isActive: queryStatus === 'success' && workgroups.length === 0,
          },
          {
            errorText: t('plainText.noWorkgroups'),
            redemptionText: t('plainText.tryAnotherKeyword'),
            isActive:
              queryStatus === 'success' &&
              filteredWorkgroups?.length === 0 &&
              !!searchText,
          },
          {
            iconColor: theme.color.intent.negative.fg.default,
            errorText: t('plainText.errorLoadingWorkgroups'),
            text: t('plainText.pleaseTryAgain'),
            isActive: queryStatus === 'error',
            Refresh: (
              <Button
                mt={theme.spacing.s8}
                onClick={() => {
                  queryClient.invalidateQueries(['workgroupSelectors'])
                }}
              >
                {t('plainText.refresh')}
              </Button>
            ),
          },
        ]
          .filter((item) => item.isActive)
          ?.map(
            ({ errorText, redemptionText, linkText, Refresh, iconColor }) => (
              <NotFound
                icon="info"
                css={{
                  textAlign: 'center',
                  overflowY: 'hidden',
                  height: '90%',
                  color: iconColor,
                }}
              >
                {errorText && (
                  <div
                    css={{
                      textTransform: 'none',
                      color: theme.color.neutral.fg.default,
                    }}
                  >
                    {titleCase({ text: errorText, language })}
                  </div>
                )}
                {redemptionText && (
                  <div
                    css={{
                      textTransform: 'none',
                      color: theme.color.neutral.fg.subtle,
                    }}
                  >
                    {titleCase({ text: redemptionText, language })}
                  </div>
                )}
                {linkText && (
                  <div
                    css={{
                      textTransform: 'none',
                    }}
                  >
                    <Link
                      href={`${window.location.origin}/authrz-web/`}
                      target="_blank"
                    >
                      {titleCase({ text: linkText, language })}
                    </Link>
                  </div>
                )}
                {Refresh}
              </NotFound>
            )
          )
      )}

      <Group mb="30px" />
    </Group>
  )
}

export default WorkgroupContent

import {
  Focus,
  formatDateTime,
  FullSizeContainer,
  FullSizeLoader,
  titleCase,
  useGetNotificationList,
} from '@willow/common'
import { insightTypes } from '@willow/common/insights/insights/types'
import { getModelInfo } from '@willow/common/twins/utils'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  getPriorityTranslatedName,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  Badge,
  Box,
  DataGrid,
  GridColDef,
  Group,
  Icon,
  Progress,
  Stack,
  Switch,
  useDisclosure,
} from '@willowinc/ui'
import _ from 'lodash'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import NotFound from '../../../components/Insights/ui/NotFound'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import NotificationSettingsActionsViewControl from './NotificationSettingsActionsViewControl'
import { useNotificationSettingsContext } from './NotificationSettingsContext'

/**
 * This function calculates the number of items that can fit in the column width
 * and returns the displayed content with the remaining count.
 */
const calculateFitItems = (contentArray, columnWidth) => {
  let currentWidth = 0

  if (contentArray.length <= 0) {
    return '--'
  }
  const { displayedContent, remainingCount } = _.reduce(
    contentArray,
    (acc, item, index) => {
      const itemWidth = item.length * 8
      if (currentWidth + itemWidth < columnWidth) {
        acc.displayedContent += (acc.displayedContent ? ', ' : '') + item
        currentWidth += itemWidth
      } else {
        acc.remainingCount = contentArray.length - Number(index)
        return acc
      }
      return acc
    },
    { displayedContent: '', remainingCount: 0 }
  )

  return `${displayedContent}${remainingCount > 0 ? ` +${remainingCount}` : ''}`
}

/**
 * A DataGrid component that displays the notifications data in a table format
 * with a switch to toggle whether setting is applicable to "Notification Settings".
 */
export default function NotificationSettingsDataGrid() {
  const {
    initializeNotificationSettings,
    onDeleteNotificationSettingsTrigger,
    notificationSettingList,
    onEditNotification,
    onToggleTriggers,
    skills,
    categories,
    myWorkgroups,
  } = useNotificationSettingsContext()

  const [notificationIdWithOpenControl, setNotificationIdWithOpenAction] =
    useState<string | undefined>(undefined)
  const [isActionsViewOpen, { toggle: onToggleActionsView }] = useDisclosure()
  const translation = useTranslation()
  const {
    t,
    i18n: { language },
  } = translation
  const { isCustomerAdmin } = useUser()
  const { data: ontology } = useOntologyInPlatform()
  const { data: { items: modelsOfInterest } = {} } = useModelsOfInterest()
  const { isLoading } = useGetNotificationList(
    {
      filterSpecifications: [],
    },
    {
      onSuccess: initializeNotificationSettings,
    }
  )
  const { scopeLookup } = useScopeSelector()

  // Do "Mute" Insights Switch even if any one of the triggers is muted
  const isMuted = notificationSettingList.some(
    (notification) => notification.isMuted
  )

  const getOnOffText = () =>
    `${titleCase({ language, text: t('plainText.off') })}/${titleCase({
      language,
      text: t('plainText.on'),
    })}`

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'focus',
        headerName: titleCase({
          language,
          text: t('plainText.focus'),
        }),
        flex: 0.8,
        renderCell: ({ row, value }) => (
          <Stack role="presentation" title={_.startCase(row.focus ?? value)}>
            {_.startCase(row.focus ?? value)}
          </Stack>
        ),
      },
      // TODO: Add focus items based on the below story
      // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/135914
      {
        field: 'focusItems',
        headerName: titleCase({
          language,
          text: t('plainText.focusItems'),
        }),
        flex: 2.5,
        renderCell: ({ row, colDef: { computedWidth } }) => {
          let content: string[] = []
          switch (row.focus) {
            case Focus.skill:
              content = skills
                .filter((skill) => row.skillIds?.includes(skill.id))
                .map((skill) => skill.name)
              break
            case Focus.skillCategory:
              content = categories
                .filter((category) =>
                  row.skillCategoryIds.includes(category.key)
                )
                .map((category) => category.value)
              break
            case Focus.twin:
              content = row.twins.map((twin) => twin.twinName)
              break
            case Focus.twinCategory:
              content = row.twinCategoryIds.map((twinCategoryId) => {
                const model = ontology?.getModelById(twinCategoryId)
                const modelInfo =
                  model && ontology && modelsOfInterest
                    ? getModelInfo(
                        model,
                        ontology,
                        modelsOfInterest,
                        translation
                      )
                    : undefined
                return (
                  modelInfo?.displayName ??
                  titleCase({ text: t('plainText.allCategories'), language })
                )
              })
              break
            default:
              content = []
          }
          const displayedContent = useMemo(
            () => calculateFitItems(content, computedWidth),
            [computedWidth, content]
          )

          return (
            <Stack role="presentation" title={displayedContent}>
              {displayedContent}
            </Stack>
          )
        },
      },
      {
        field: 'priorities',
        headerName: titleCase({
          language,
          text: t('labels.priority'),
        }),
        flex: 1.5,
        renderCell: ({ row }) => {
          const priorityNames =
            (row.priorityIds ?? [])
              .map((priority) => getPriorityTranslatedName(t, priority))
              .join(', ') || '--'
          return (
            <Stack role="presentation" title={priorityNames}>
              {priorityNames}
            </Stack>
          )
        },
      },
      {
        field: 'locations',
        headerName: titleCase({
          language,
          text: t('plainText.location'),
        }),
        flex: 1.5,
        renderCell: ({ row, value }) => {
          const locationNames =
            (row.locations ?? value)
              ?.reduce((acc, location) => {
                const name = scopeLookup[location]?.twin?.name
                if (name) {
                  acc.push(name)
                }
                return acc
              }, [])
              .join(', ') || '--'
          return (
            <Stack role="presentation" title={locationNames}>
              {locationNames}
            </Stack>
          )
        },
      },
      {
        field: 'personalWorkgroup',
        headerName: `${titleCase({
          language,
          text: t('plainText.personal'),
        })}/${titleCase({
          language,
          text: t('headers.workgroup'),
        })}`,
        flex: 1,
        renderCell: ({ row, value }) => (
          <Stack role="presentation" title={_.startCase(row.type ?? value)}>
            {_.startCase(row.type ?? value)}
          </Stack>
        ),
      },
      ...(isCustomerAdmin
        ? [
            {
              field: 'workgroupIds',
              headerName: titleCase({
                language,
                text: t('plainText.workgroupAccess'),
              }),
              flex: 1,
              renderCell: ({ row, value }) => {
                const totalUsers = (row.workgroupIds ?? value ?? []).length
                const content =
                  totalUsers > 0 ? `${totalUsers} ${t('labels.users')}` : '--'
                return (
                  <Stack role="presentation" title={content}>
                    {content}
                  </Stack>
                )
              },
            },
            {
              field: 'workgroupNotification',
              headerName: `${getOnOffText()} (${t('headers.workgroup')})`,
              flex: 1,
              renderCell: ({ row, value }) =>
                row.type === 'personal' ? (
                  <Box ml="s4">--</Box>
                ) : (
                  <Switch
                    mb="0px"
                    labelPosition="right"
                    disabled={row.isMuted}
                    checked={row.isEnabled ?? value}
                    onClick={() => {
                      onToggleTriggers({
                        baseUrl: `/notifications/triggers/${row.id}`,
                        //  Toggle the 'isEnabled' status (Available only for Admin)
                        params: { isEnabled: !row.isEnabled },
                      })
                    }}
                    label={t(
                      row.isEnabled ?? value ? 'plainText.on' : 'plainText.off'
                    )}
                  />
                ),
            },
          ]
        : ([] as any)),
      {
        field: 'channels',
        headerName: titleCase({
          language,
          text: t('plainText.channel'),
        }),
        flex: 0.6,
        renderCell: ({ row, value }) => (
          <Badge
            variant="outline"
            color={row.isMuted ? 'gray' : 'purple'}
            size="md"
          >
            {_.startCase(row.channels ?? value)}
          </Badge>
        ),
      },
      {
        field: 'createdDate',
        headerName: titleCase({
          language,
          text: t('labels.created'),
        }),
        flex: 0.8,
        renderCell: ({ row, value }) => {
          const content = formatDateTime({
            value: row.createdDate ?? value,
            language,
          })
          return (
            <Stack role="presentation" title={content}>
              {content}
            </Stack>
          )
        },
      },
      {
        field: 'personalNotification',
        headerName: isCustomerAdmin
          ? `${getOnOffText()} (${titleCase({
              text: t('plainText.you'),
              language,
            })})`
          : `${getOnOffText()}`,
        flex: isCustomerAdmin ? 0.8 : 0.5,
        renderCell: ({ row, value }) => {
          const isChecked = row.isEnabledForUser ?? value

          // check if trigger has any common workgroupIds with admin's workgroupIds.
          const myWorkgroupIds = new Set(myWorkgroups.map((item) => item.id))
          const isWorkgroupsMatch = row.workgroupIds.every((workgroupId) =>
            myWorkgroupIds.has(workgroupId)
          )

          return !isWorkgroupsMatch ? (
            <Box ml="s4">--</Box>
          ) : (
            <Switch
              mb="0px"
              labelPosition="right"
              checked={isChecked}
              disabled={row.isMuted}
              label={t(isChecked ? 'plainText.on' : 'plainText.off')}
              onClick={() =>
                onToggleTriggers({
                  baseUrl: `/notifications/triggers/${row.id}`,
                  // toggle the 'isEnabledForUser' status.
                  params: { isEnabledForUser: !row.isEnabledForUser },
                })
              }
            />
          )
        },
      },
      {
        field: 'id',
        headerName: '',
        renderCell: ({ row }) => (
          // line-height: 0px; helps to position actions view control icon in the center
          // vertically and horizontally
          <FullSizeContainer css="line-height: 0px;">
            <NotificationSettingsActionsViewControl
              onEditNotificationSettings={() => {
                onEditNotification(row.id)
              }}
              opened={
                isActionsViewOpen && notificationIdWithOpenControl === row.id
              }
              onToggleActionsView={(updatedActionsViewState) => {
                onToggleActionsView()
                setNotificationIdWithOpenAction(
                  updatedActionsViewState ? row.id : undefined
                )
              }}
              position="bottom-end"
              withinPortal
              isCustomerAdmin={isCustomerAdmin}
              onDeleteNotificationSettings={() =>
                onDeleteNotificationSettingsTrigger(row)
              }
            >
              <Icon
                tw="cursor-pointer w-[100%]"
                data-testid={`actionViewControl-${row.id}`}
                icon="more_vert"
              />
            </NotificationSettingsActionsViewControl>
          </FullSizeContainer>
        ),
        disableSortBy: true,
        width: 44,
      },
    ],
    [
      categories,
      skills,
      modelsOfInterest,
      ontology,
      notificationIdWithOpenControl,
      isActionsViewOpen,
      isCustomerAdmin,
      scopeLookup,
      t,
      language,
    ]
  )

  return (
    <Stack h="100%" gap={0}>
      {isLoading ? (
        <FullSizeLoader />
      ) : (
        <>
          <Group>
            <StyledText
              css={css(({ theme }) => ({
                marginTop: theme.spacing.s12,
                marginLeft: theme.spacing.s12,
              }))}
            >
              {t('headers.insights')}
            </StyledText>
            <Switch
              label={t(isMuted ? 'plainText.off' : 'plainText.on')}
              labelPosition="right"
              h="s48"
              m="s16"
              mb="0"
              mr="0"
              ml="0"
              display="flex"
              defaultChecked={!isMuted}
              checked={!isMuted}
              onClick={() =>
                onToggleTriggers({
                  baseUrl: `notifications/triggers/toggle`,
                  params: { source: 'insight' },
                })
              }
            />
          </Group>
          <StyledDataGrid
            rows={notificationSettingList}
            columns={columns}
            hideFooter
            hideFooterRowCount
            disableRowSelectionOnClick={false}
            slots={{
              loadingOverlay: Progress, // Custom loading to override with MUI loading icon
              noRowsOverlay: () => (
                <NotFound
                  message={titleCase({
                    language,
                    text: t('plainText.noNotificationCreated'),
                  })}
                />
              ),
            }}
            getRowClassName={({ row }) => (row.isMuted ? 'trigger-mute' : '')}
            disableMultipleRowSelection
          />
        </>
      )}
    </Stack>
  )
}

const StyledText = styled.div(({ theme }) => ({
  ...theme.font.heading.lg,
}))

const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  '&&&': {
    border: '0px',
  },
  // Overriding the existing styling to show the resize icon in the table header
  '.MuiDataGrid-columnSeparator': {
    display: 'block',
    color: theme.color.neutral.fg.default,

    '&:hover': {
      color: theme.color.neutral.fg.highlight,
    },

    '> svg': {
      height: '56px',
      display: 'block',
    },
  },

  '& .trigger-mute': {
    color: theme.color.state.disabled.fg,

    '&:hover': {
      backgroundColor: 'transparent',
      color: theme.color.state.disabled.fg,
    },

    '&.MuiDataGrid-row.Mui-hovered': {
      backgroundColor: 'transparent',
    },
  },
}))

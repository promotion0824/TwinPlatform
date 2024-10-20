import { iconMap as insightCategoryMap } from '@willow/common/insights/component'
import { getModelInfo } from '@willow/common/twins/utils'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { TwinChip, useDateTime } from '@willow/ui'
import {
  Box,
  Button,
  Group,
  Icon,
  Illustration,
  Indicator,
  Radio,
  Stack,
  Tooltip,
  useTheme,
} from '@willowinc/ui'
import { WillowStyleProps } from '@willowinc/ui/src/lib/utils'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import routes from '../../../platform/src/routes'
import { FullSizeContainer } from '../components'
import { Ontology } from '../twins/view/models'
import titleCase from '../utils/titleCase'
import { Notification, NotificationSource, NotificationStatus } from './types'

const NotificationComponent = ({
  notification,
  modelsOfInterest,
  ontology,
  onNotificationsStatusesChange,
  isMarkingAsRead,
}: NotificationBaseProps & {
  ontology?: Ontology
  modelsOfInterest?: ModelOfInterest[]
}) => {
  const theme = useTheme()
  const translation = useTranslation()
  const history = useHistory()

  const { source, properties: { modelId, id: notificationSourceId } = {} } =
    notification
  const model = modelId ? ontology?.getModelById(modelId) : undefined
  const modelInfo =
    model && ontology && modelsOfInterest
      ? getModelInfo(model, ontology, modelsOfInterest, translation)
      : undefined

  return (
    <Group
      w="100%"
      gap={0}
      // The following style is to support custom style for "Radio"
      // - no border when not hovered
      // - if hovered on container of 'Radio', border shown with 'color.intent.primary.border.default'
      // - if hovered directly on 'Radio', background of Radio changed to 'color.intent.primary.bg.subtle.hovered'
      css={{
        cursor: 'pointer',
        '& *': {
          outline: 'none',
          '& [type="radio"]': {
            border: 'none',
          },
        },
        backgroundColor: theme.color.neutral.bg.panel.default,
        borderRadius: theme.spacing.s2,
        '&:hover': {
          backgroundColor: theme.color.neutral.bg.panel.hovered,
          '& [type="radio"]': {
            border: 'calc(0.0625rem * var(--mantine-scale)) solid',
            borderColor: theme.color.intent.primary.border.default,
          },
        },
      }}
      onClick={() => {
        const sourceId = notification?.sourceId ?? notificationSourceId
        if (source === NotificationSource.Insight && sourceId) {
          history.push(routes.insights_insight__insightId(sourceId))
        }
      }}
    >
      <NotificationContent
        notification={notification}
        modelOfInterest={modelInfo?.modelOfInterest}
        onNotificationsStatusesChange={onNotificationsStatusesChange}
        isMarkingAsRead={isMarkingAsRead}
      />
    </Group>
  )
}

const GrayDot = () => (
  <Indicator position="middle-center" intent="secondary" size={3}>
    <span />
  </Indicator>
)

export default NotificationComponent

/**
 * A vertically aligned stack of notification contents.
 */
const NotificationContent = ({
  modelOfInterest,
  notification,
  onNotificationsStatusesChange,
  isMarkingAsRead = false,
}: NotificationBaseProps & { modelOfInterest?: ModelOfInterest }) => {
  const dateTime = useDateTime()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const {
    title,
    createdDateTime,
    properties: { category: insightCategory = '', twinName } = {},
    source: notificationType,
    state,
  } = notification
  const theme = useTheme()

  return (
    <Stack px="s16" py="s8" flex={1}>
      <Group>
        <Tooltip
          label={titleCase({
            text: t(
              notification.state === NotificationStatus.New
                ? 'plainText.markAsRead'
                : 'plainText.markAsUnread'
            ),
            language,
          })}
          withArrow
          withinPortal
        >
          <Radio
            onClick={(e) => {
              e.stopPropagation()
              onNotificationsStatusesChange(
                [notification.id],
                state === NotificationStatus.New
                  ? NotificationStatus.Open
                  : NotificationStatus.New
              )
            }}
            onChange={() => _.noop} // For accessibility
            checked={!isMarkingAsRead && state === NotificationStatus.New}
            css={{
              '& input': {
                color:
                  !isMarkingAsRead && state === NotificationStatus.New
                    ? 'inherit'
                    : 'transparent',
              },
              '& [type="radio"]': {
                cursor: 'pointer',
                border: 'none',
                '&:hover': {
                  backgroundColor: theme.color.intent.primary.bg.subtle.hovered,
                },
              },
            }}
          />
        </Tooltip>
        {/* 
          Max 2 lines for title, overflow with ellipsis as per design
        */}
        <Box
          css={{
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
          }}
        >
          {title}
        </Box>
      </Group>
      <TwinChip
        variant="instance"
        modelOfInterest={modelOfInterest}
        text={twinName}
        highlightOnHover
        css={{
          width: 'fit-content',
          marginLeft: theme.spacing.s24,
        }}
      />
      <Group
        css={{
          ...theme.font.body.xs.regular,
          marginLeft: theme.spacing.s24,
          color: theme.color.neutral.fg.muted,
        }}
      >
        <Box>
          {dateTime(createdDateTime).format('ago', undefined, language)}
        </Box>
        {notificationType && (
          <>
            <GrayDot />
            {titleCase({
              text:
                notificationType === NotificationSource.Insight
                  ? t('headers.insights')
                  : notificationType === NotificationSource.Ticket
                  ? t('headers.tickets')
                  : '',
              language,
            })}
          </>
        )}
        {insightCategoryMap?.[insightCategory]?.value && (
          <>
            <GrayDot />
            <Box>{insightCategoryMap[insightCategory].value}</Box>
          </>
        )}
      </Group>
    </Stack>
  )
}

export const NoNotifications = ({
  w = '100%',
  search,
  message,
}: {
  w?: WillowStyleProps['w']
  search?: string
  message?: string
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  return (
    <Stack w={w} align="center" p="s16" data-testid="no-notifications">
      <Illustration illustration={search ? 'no-results' : 'no-data'} w={108} />
      <Box
        c="neutral.fg.default"
        w="207px"
        css={{
          whiteSpace: 'wrap',
          textAlign: 'center',
        }}
      >
        {search ? (
          <>
            {titleCase({
              text: t('plainText.noMatchingResults'),
              language,
            })}
            <br />
            {titleCase({
              text: t('plainText.tryAnotherKeyword'),
              language,
            })}
          </>
        ) : (
          message || t('plainText.noNotificationsLastThirtyDays')
        )}
      </Box>
    </Stack>
  )
}

export const ErrorReload = ({ onReload }: { onReload: () => void }) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  return (
    <FullSizeContainer>
      <Stack w="100%" align="center" p="s16">
        <Icon icon="info" c="intent.negative.fg.default" />
        <Box c="neutral.fg.default">
          {titleCase({
            text: t('plainText.errorLoadingNotifications'),
            language,
          })}
        </Box>
        <Box c="neutral.fg.subtle">{t('plainText.pleaseTryAgain')}</Box>
        <Button mt="s8" onClick={onReload}>
          {t('plainText.refresh')}
        </Button>
      </Stack>
    </FullSizeContainer>
  )
}

interface NotificationBaseProps {
  notification: Notification
  onNotificationsStatusesChange: (
    notificationIds: string[],
    state: NotificationStatus
  ) => void
  isMarkingAsRead?: boolean
}

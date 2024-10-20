/* eslint-disable no-empty-pattern */
import tw, { styled } from 'twin.macro'
import _ from 'lodash'
import { Text, getPriorityTranslatedName } from '@willow/ui'
import { DateTime } from 'luxon'
import { Avatar, TextInput } from '@willowinc/ui'
import { TFunction, useTranslation } from 'react-i18next'
import { getFullNameInitials } from '@willow/common/utils/activityUtils'
import { InsightActions } from '../../../../platform/src/components/Insights/ui/ActionsViewControl'
import InsightWorkflowStatusPill from '../../../../platform/src/components/InsightStatusPill/InsightWorkflowStatusPill'
import {
  Activity,
  ActivityKey,
  InsightWorkflowStatus,
  ParamsDictionary,
} from '../insights/types'
import { TextWithTooltip } from './index'

const DATETIME_SHORT_WITH_SECONDS = 'M/d/yyyy h:mm:ss a'

/**
 * This component displays an Avatar icon using the full name and size and
 * it optionally shows the full name as a suffix based on the showName boolean value
 */
const CustomAvatar = ({
  fullName,
  size,
  showName = false,
  isNoMargin = false,
  t,
  trailingText,
}: {
  fullName: string
  size: string
  t: TFunction
  isNoMargin?: boolean
  showName?: boolean
  trailingText?: string
}) => {
  const updatedFullName = fullName || t('plainText.unassigned')
  return (
    <AvatarWrapper $isNoMargin={isNoMargin}>
      <Avatar shape="rectangle" color="blue" size={size} variant="subtle">
        {getFullNameInitials(updatedFullName)}
      </Avatar>
      <span tw="ml-[8px] truncate">
        {`${showName ? updatedFullName : ''} ${trailingText || ''}`}
      </span>
    </AvatarWrapper>
  )
}

/**
 * This component displays the ticket modified header value based on the Activity list
 * If there are multiple changes in the existing ticket, it shows a generic message
 * However, if only one value is modified, the header content changes based on that value
 */
const TicketModifiedHeader = ({
  activities,
  t,
}: {
  activities: Activity[]
  t: TFunction
}) => (
  <>
    {activities.length > 1 ? (
      <StyledSpan>
        {`${_.capitalize(t('headers.ticket'))} ${t(
          'plainText.updatedWithMultipleChanges'
        )}`}
      </StyledSpan>
    ) : (
      (() => {
        switch (activities[0]?.key) {
          case ActivityKey.Description: {
            return (
              <StyledSpan>
                {`${t('headers.ticket')} ${_.lowerCase(activities[0]?.key)} ${t(
                  'plainText.updated'
                )}`}
              </StyledSpan>
            )
          }
          case ActivityKey.DueDate: {
            return (
              <StyledSpan>
                {`${_.capitalize(t('headers.ticket'))} ${t(
                  'plainText.dueDateSetTo'
                )} ${DateTime.fromFormat(
                  activities[0]?.value,
                  DATETIME_SHORT_WITH_SECONDS
                ).toLocaleString(DateTime.DATE_FULL)}`}
              </StyledSpan>
            )
          }
          case ActivityKey.Priority:
            return (
              <StyledSpan>
                {`${_.capitalize(t('headers.ticket'))} ${_.lowerCase(
                  activities[0]?.key
                )} ${t('plainText.setTo')} ${getPriorityTranslatedName(
                  t,
                  Number(activities[0]?.value)
                )}`}
              </StyledSpan>
            )
          case ActivityKey.Status: {
            return (
              <StyledSpan>
                {`${_.capitalize(t('headers.ticket'))} ${_.lowerCase(
                  activities[0]?.key
                )} ${t('plainText.setTo')} ${_.startCase(
                  activities[0]?.value
                )}`}
              </StyledSpan>
            )
          }
          case ActivityKey.AssigneeName: {
            return (
              <StyledSpan tw="flex items-center">
                {`${_.capitalize(t('headers.ticket'))} ${t(
                  'labels.assignee'
                )} ${t('plainText.setTo')}`}
                <CustomAvatar
                  fullName={activities[0]?.value}
                  size="md"
                  t={t}
                  showName
                />
              </StyledSpan>
            )
          }
          default:
            return null
        }
      })()
    )}
  </>
)

const StyledSpan = styled.span(({ theme }) => ({
  paddingRight: theme.spacing.s8,
}))

/**
 * This component displays a text area field along with a label for new/modified ticket
 */
const TicketTextArea = ({
  activity,
  fullName,
  t,
  className,
}: {
  activity: Activity
  fullName: string
  t: TFunction
  className?: string
}) => (
  <StyledList className={className}>
    <span>{activity.key}</span>
    <StyledTextInput
      value={activity.value}
      prefix={<CustomAvatar fullName={fullName} size="sm" t={t} />}
      readOnly
    />
  </StyledList>
)

/**
 * This component displays the formatted due date along with a label for new/modified ticket
 */
const TicketDueDate = ({
  activity,
  t,
  className,
}: {
  activity: Activity
  t: TFunction
  className?: string
}) => (
  <StyledList className={className}>
    <span>{_.startCase(t('labels.dueDate'))}</span>
    {activity.value
      ? DateTime.fromFormat(
          activity.value,
          DATETIME_SHORT_WITH_SECONDS
        ).toLocaleString(DateTime.DATE_FULL)
      : _.startCase(t('plainText.noDueDate'))}
  </StyledList>
)

/**
 * This component displays the status along with a label for new/modified ticket
 */
const TicketStatus = ({
  activity,
  className,
}: {
  activity: Activity
  className?: string
}) => (
  <StyledList className={className}>
    <span>{activity.key}</span>
    {_.startCase(activity.value)}
  </StyledList>
)

/**
 * This component displays the Avatar of the Assignee with a label for new/modified ticket
 */
const TicketAssignee = ({
  activity,
  t,
  className,
}: {
  activity: Activity
  t: TFunction
  className?: string
}) => (
  <StyledList className={className}>
    <span>{t('labels.assignee')}</span>
    <CustomAvatar
      fullName={activity.value}
      size="md"
      showName
      t={t}
      isNoMargin
    />
  </StyledList>
)

/**
 * This component will be displayed as part of ticket sub section for new/modified ticket
 * The content will change based on the activity list
 */
const TicketSubSection = ({
  activities,
  fullName,
  t,
  className,
}: {
  activities: Activity[]
  fullName: string
  t: TFunction
  className?: string
}) => (
  <>
    {activities.map((activity, index) => {
      const key = `${activity.key}-${activity.value}-${index}}`
      switch (activity.key) {
        case ActivityKey.Comments:
        case ActivityKey.Description: {
          return (
            <TicketTextArea
              activity={activity}
              key={key}
              t={t}
              fullName={fullName}
              className={className}
            />
          )
        }
        case ActivityKey.AssigneeName: {
          return (
            <TicketAssignee
              className={className}
              activity={activity}
              key={key}
              t={t}
            />
          )
        }
        case ActivityKey.Status: {
          return (
            <TicketStatus className={className} activity={activity} key={key} />
          )
        }
        case ActivityKey.DueDate: {
          return (
            <TicketDueDate
              className={className}
              activity={activity}
              key={key}
              t={t}
            />
          )
        }
        default:
          return null
      }
    })}
  </>
)

/**
 * This component showcases the name of the Assignee along with a link to the Ticket Summary
 * If the Ticket Summary is available, it will be displayed; if not, the Ticket ID will be shown as static text instead
 * When clicked, the link will open a modal containing detailed information about the ticket.
 */
const TicketLink = ({
  fullName,
  ticketId,
  ticketSummary,
  className,
  onTicketLinkClick,
}: {
  fullName: string
  ticketId?: string
  ticketSummary?: string
  className?: string
  onTicketLinkClick?: (option: ParamsDictionary) => void
}) => {
  const { t } = useTranslation()
  const text =
    ticketSummary ||
    `${_.capitalize(t('headers.ticket'))} ${_.toUpper(t('plainText.id'))}`

  return (
    <StyledSubSection className={className}>
      <span>
        <CustomAvatar
          fullName={fullName}
          showName
          size="sm"
          t={t}
          trailingText={t('plainText.updated')}
        />
      </span>
      {ticketId && (
        <TicketLinkContainer
          onClick={() =>
            onTicketLinkClick?.({
              ticketId,
              action: InsightActions.ticket,
            })
          }
        >
          <TextWithTooltip
            text={text}
            tooltipWidth="200px"
            isTitleCase={false}
          />
        </TicketLinkContainer>
      )}
    </StyledSubSection>
  )
}

/**
 * This component displays the activity header section irrespective of the Activity type
 */
const ActivityHeader = ({
  text,
  date,
  isStatus = false,
  status,
}: {
  text: string | React.ReactElement
  date: string
  isStatus?: boolean
  status?: InsightWorkflowStatus
}) => (
  <>
    <FormattedText>
      <span tw="mr-[8px]">{text}</span>
      {isStatus && (
        <InsightWorkflowStatusPill
          size="lg"
          lastStatus={_.lowerFirst(status) as InsightWorkflowStatus}
        />
      )}
    </FormattedText>
    <FormattedDate date={date} />
  </>
)

/**
 * This component displays the formatted date shown in Activity header section
 */
const FormattedDate = ({ date }) => (
  <StyledFormattedDate tw="truncate">{date}</StyledFormattedDate>
)

const StyledSubSection = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
}))

const StyledFormattedDate = styled.span(({ theme }) => ({
  ...theme.font.heading.sm,
  float: 'right',
  textTransform: 'initial',
  color: theme.color.neutral.fg.default,
  lineHeight: theme.font.display.md.light.fontSize,
}))

const FormattedText = styled(Text)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  textTransform: 'initial',
  ...theme.font.heading.lg,
  whiteSpace: 'normal',
  height: '28px',
  '&&&': {
    lineHeight: '28px',
  },
}))

const StyledList = styled.div(({ theme }) => ({
  padding: theme.spacing.s8,
  display: 'flex',

  '> span:first-child': {
    width: '267px',
    color: theme.color.neutral.fg.default,
    ...theme.font.body.md.semibold,
  },
  '> :nth-child(2)': {
    flex: 1,
  },
}))

const StyledTextInput = styled(TextInput)(({ theme }) => ({
  '&&& input': {
    textOverflow: 'ellipsis',
    fontStyle: 'italic',
    fontSize: theme.font.body.md.regular.fontSize,
    backgroundColor: theme.color.neutral.bg.accent.default,
    color: theme.color.neutral.fg.default,
  },
}))

const TicketLinkContainer = styled.span(({ theme }) => ({
  marginLeft: theme.spacing.s8,
  cursor: 'pointer',
  textDecoration: 'underline',
  maxWidth: '374px',
  overflow: 'hidden',
  whiteSpace: 'nowrap',
  textOverflow: 'ellipsis',
  ...theme.font.body.md,
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
}))

const AvatarWrapper = styled.span<{
  $isNoMargin: boolean
}>(({ $isNoMargin = false, theme }) => ({
  display: 'flex',
  margin: $isNoMargin ? '0' : `0 ${theme.spacing.s6}`,
  alignItems: 'center',
}))

export {
  ActivityHeader,
  TicketModifiedHeader,
  TicketSubSection,
  TicketLink,
  CustomAvatar,
  StyledTextInput,
  StyledList,
  TicketTextArea,
}

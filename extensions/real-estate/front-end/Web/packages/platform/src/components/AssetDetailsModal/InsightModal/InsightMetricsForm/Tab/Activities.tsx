/* eslint-disable complexity */
import {
  Option,
  Select,
  InsightSorts,
  useLanguage,
  caseInsensitiveEquals,
} from '@willow/ui'
import { Fragment } from 'react'
import _ from 'lodash'
import { TFunction, useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import {
  ActivityKey,
  ActivityType,
  InsightPriority,
  InsightWorkflowActivity,
  InsightWorkflowStatus,
  SortBy,
  ParamsDictionary,
} from '@willow/common/insights/insights/types'
import {
  Container,
  InsightDetail,
  PriorityName,
  ActivityCount,
} from '@willow/common/insights/component'
import {
  groupAttachments,
  reorderInsightActivity,
} from '@willow/common/utils/activityUtils'
import {
  ActivityHeader,
  CustomAvatar,
  StyledList,
  StyledTextInput,
  TicketLink,
  TicketModifiedHeader,
  TicketSubSection,
  TicketTextArea,
} from '@willow/common/insights/component/Activity'
import {
  InsightCostImpactPropNames,
  formatDateTime,
  getImpactScore,
} from '@willow/common'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { Icon } from '@willowinc/ui'
import { css } from 'styled-components'
import styles from '../../../../Insights/InsightNode/LeftPanel.css'

const Activities = ({
  activities = [],
  sortBy = SortBy.desc,
  onSortByChange,
  onTicketLinkClick,
  timeZone,
}: {
  activities?: InsightWorkflowActivity[]
  sortBy?: SortBy
  onSortByChange?: (option?: SortBy) => void
  onTicketLinkClick?: (option: ParamsDictionary) => void
  timeZone?: string
}) => {
  const { language } = useLanguage()
  const { t } = useTranslation()
  const sortedActivities = groupAttachments(activities, sortBy)

  const options = [
    {
      optionText: InsightSorts.NEWEST,
      option: SortBy.desc,
    },
    {
      optionText: InsightSorts.OLDEST,
      option: SortBy.asc,
    },
  ]

  return (
    <>
      {sortedActivities.length > 0 && (
        <ActivitiesContainer>
          <StyledSelect
            value={t('interpolation.sortByItem', {
              item:
                sortBy === SortBy.asc
                  ? InsightSorts.OLDEST
                  : InsightSorts.NEWEST,
            })}
          >
            {options.map(({ option, optionText }) => (
              <Option
                key={option}
                value={option}
                onClick={() => onSortByChange?.(option)}
              >
                {optionText}
              </Option>
            ))}
          </StyledSelect>
          <Container tw="p-4 gap-3">
            {sortedActivities.map((item, index) => {
              const key = `${item.activityDate}-${index}`
              return (
                <Fragment key={key}>
                  {getActivityContent(
                    item,
                    language,
                    t,
                    onTicketLinkClick,
                    timeZone
                  )}
                </Fragment>
              )
            })}
          </Container>
        </ActivitiesContainer>
      )}
    </>
  )
}

export default Activities

const getActivityContent = (
  item: InsightWorkflowActivity,
  language: Language,
  t: TFunction,
  onTicketLinkClick?: (option: ParamsDictionary) => void,
  timeZone?: string
) => {
  const fullName: string = item?.fullName || t('plainText.unassigned')

  const formattedActivityDate = formatDateTime({
    value: item.activityDate,
    language,
    timeZone,
  })

  switch (item.activityType) {
    case ActivityType.InsightActivity: {
      const insightStatus = _.lowerFirst(
        _.find(item.activities, { key: ActivityKey.Status })?.value ?? ''
      ) as InsightWorkflowStatus

      switch (insightStatus) {
        case 'new': {
          const sortedActivities = reorderInsightActivity(item.activities)
          return (
            <InsightDetail
              isDefaultExpanded={false}
              headerText={
                <ActivityHeader
                  text={_.capitalize(t('plainText.insightActivated'))}
                  date={formattedActivityDate}
                  status={insightStatus}
                  isStatus
                />
              }
              sectionHeaderClassName={styles.flex}
            >
              <StyledSubSection>
                {sortedActivities.map(({ key, value }, index) => {
                  const itemKey = `${key}-${value}-${index}}`
                  if (value) {
                    switch (key) {
                      case ActivityKey.PreviouslyResolved:
                      case ActivityKey.PreviouslyIgnored: {
                        const isPreviouslyIgnored = caseInsensitiveEquals(
                          ActivityKey.PreviouslyIgnored,
                          key
                        )
                        return (
                          <>
                            {value === 'True' && (
                              <StyledList key={itemKey} className={styles.flex}>
                                <span>
                                  {_.capitalize(t('plainText.activity'))}
                                </span>
                                <span tw="flex items-center">
                                  {_.startCase(key)}
                                  <ActivityCount
                                    key={key}
                                    icon={
                                      isPreviouslyIgnored
                                        ? 'do_not_disturb_on'
                                        : 'check_circle'
                                    }
                                    activityCount={1}
                                    tooltipText={t(
                                      'interpolation.previouslyItemHistory',
                                      {
                                        itemHistory: isPreviouslyIgnored
                                          ? t('headers.ignored')
                                          : t('headers.resolved'),
                                      }
                                    )}
                                    filled={false}
                                  />
                                </span>
                              </StyledList>
                            )}
                          </>
                        )
                      }
                      case ActivityKey.Priority: {
                        return (
                          <StyledList key={itemKey} className={styles.flex}>
                            <span>{t('labels.priority')}</span>
                            <div tw="flex-1">
                              <PriorityName
                                insightPriority={
                                  Number(value) as InsightPriority
                                }
                              />
                            </div>
                          </StyledList>
                        )
                      }
                      case ActivityKey.ImpactScores: {
                        const formattedImpactScores = JSON.parse(value)
                        return formattedImpactScores.map(({ fieldId }, i) => {
                          const nestedKey = `${itemKey}-${fieldId}-${i}`
                          switch (fieldId) {
                            case 'daily_avoidable_energy': {
                              return (
                                <StyledList
                                  key={nestedKey}
                                  className={styles.flex}
                                >
                                  <span>
                                    {_.capitalize(
                                      t(
                                        'interpolation.avoidableExpensePerYear',
                                        {
                                          expense: _.capitalize(
                                            t('plainText.energy')
                                          ),
                                        }
                                      )
                                    )}
                                  </span>
                                  <span>
                                    {getImpactScore({
                                      impactScores: formattedImpactScores,
                                      scoreName:
                                        InsightCostImpactPropNames.dailyAvoidableEnergy,
                                      multiplier: 365,
                                      language,
                                      decimalPlaces: 0,
                                    })}
                                  </span>
                                </StyledList>
                              )
                            }
                            case 'daily_avoidable_cost': {
                              return (
                                <StyledList
                                  key={nestedKey}
                                  className={styles.flex}
                                >
                                  <span>
                                    {_.capitalize(
                                      t(
                                        'interpolation.avoidableExpensePerYear',
                                        {
                                          expense: _.capitalize(
                                            t('plainText.cost')
                                          ),
                                        }
                                      )
                                    )}
                                  </span>
                                  <span>
                                    {getImpactScore({
                                      impactScores: formattedImpactScores,
                                      scoreName:
                                        InsightCostImpactPropNames.dailyAvoidableCost,
                                      multiplier: 365,
                                      language,
                                      decimalPlaces: 0,
                                    })}
                                  </span>
                                </StyledList>
                              )
                            }
                            default:
                              return ''
                          }
                        })
                      }
                      case ActivityKey.OccurrenceStarted: {
                        const occurrenceEndDate = _.find(sortedActivities, {
                          key: ActivityKey.OccurrenceEnded,
                        })?.value
                        // showing the occurrence time range only if start and end date are present
                        return (
                          value &&
                          occurrenceEndDate && (
                            <StyledList key={key} className={styles.flex}>
                              <span>
                                {_.startCase(
                                  t('plainText.occurrenceTimeRange')
                                )}
                              </span>
                              <span>
                                {formatDateTime({ value, language, timeZone })}
                                <span tw="px-1">-</span>
                                {formatDateTime({
                                  value: occurrenceEndDate,
                                  language,
                                  timeZone,
                                })}
                              </span>
                            </StyledList>
                          )
                        )
                      }
                      default:
                        return ''
                    }
                  }
                  return null
                })}
              </StyledSubSection>
            </InsightDetail>
          )
        }
        case 'inProgress': {
          return (
            <InsightDetail
              isDefaultExpanded={false}
              headerText={
                <ActivityHeader
                  text={_.capitalize(t('plainText.insightStatusSetTo'))}
                  date={formattedActivityDate}
                  status={insightStatus}
                  isStatus
                />
              }
              sectionHeaderClassName={styles.flex}
            >
              <StyledSubSection>
                <StyledFlex className={styles.flex}>
                  <CustomAvatar
                    fullName={fullName}
                    showName
                    size="sm"
                    t={t}
                    trailingText={`${t('plainText.actioned')} ${t(
                      'headers.insight'
                    )}`}
                  />
                </StyledFlex>
              </StyledSubSection>
            </InsightDetail>
          )
        }
        case 'resolved': {
          const resolveComment = _.lowerFirst(
            _.find(item.activities, { key: ActivityKey.Reason })?.value ?? ''
          )

          return (
            <InsightDetail
              isDefaultExpanded={false}
              headerText={
                <ActivityHeader
                  text={_.capitalize(t('plainText.insightResolved'))}
                  date={formattedActivityDate}
                  status={insightStatus}
                  isStatus
                />
              }
              sectionHeaderClassName={styles.flex}
            >
              <StyledSubSection>
                <StyledFlex className={styles.flex}>
                  {!resolveComment &&
                  caseInsensitiveEquals(item.sourceType, 'app')
                    ? t('interpolation.autoResolveActivityMessage', {
                        appName: _.startCase(item.appName),
                      })
                    : !resolveComment &&
                      caseInsensitiveEquals(item.sourceType, 'willow')
                    ? t('interpolation.resolveMessageUser', {
                        user: fullName,
                      })
                    : _.capitalize(resolveComment)}
                </StyledFlex>
              </StyledSubSection>
            </InsightDetail>
          )
        }
        case 'ignored': {
          return (
            <InsightDetail
              isDefaultExpanded={false}
              headerText={
                <ActivityHeader
                  text={_.capitalize(t('plainText.insightIgnored'))}
                  date={formattedActivityDate}
                  status={insightStatus}
                  isStatus
                />
              }
              sectionHeaderClassName={styles.flex}
            >
              <StyledSubSection>
                <StyledFlex className={styles.flex}>
                  <CustomAvatar fullName={fullName} showName size="sm" t={t} />
                  <span
                    tw="flex items-center"
                    className={styles.smallMarginLeft}
                  >
                    {t('headers.ignored')}
                    <ActivityCount
                      icon="do_not_disturb_on"
                      activityCount={1}
                      tooltipText={t('interpolation.previouslyItemHistory', {
                        itemHistory: t('headers.ignored'),
                      })}
                      filled={false}
                    />
                  </span>
                </StyledFlex>
              </StyledSubSection>
            </InsightDetail>
          )
        }
        case 'readyToResolve': {
          return (
            <InsightDetail
              isDefaultExpanded={false}
              headerText={
                <ActivityHeader
                  text={_.capitalize(t('plainText.insightStatusSetTo'))}
                  date={formattedActivityDate}
                  status={insightStatus}
                  isStatus
                />
              }
              sectionHeaderClassName={styles.flex}
            >
              <StyledSubSection>
                <StyledFlex className={styles.flex}>
                  {t('interpolation.autoResolveActivityMessage', {
                    appName: _.startCase(item.appName),
                  })}
                </StyledFlex>
              </StyledSubSection>
            </InsightDetail>
          )
        }
        default:
          return <></>
      }
    }
    case ActivityType.NewTicket: {
      return (
        <InsightDetail
          isDefaultExpanded={false}
          headerIcon={<Icon icon="assignment" size={24} tw="mt-[2px]" />}
          headerText={
            <ActivityHeader
              text={_.capitalize(t('plainText.ticketCreated'))}
              date={formattedActivityDate}
            />
          }
          sectionHeaderClassName={styles.flex}
        >
          <TicketLink
            fullName={fullName}
            ticketId={item.ticketId}
            ticketSummary={item.ticketSummary}
            className={styles.ticketLink}
            onTicketLinkClick={onTicketLinkClick}
          />
          <StyledSubSection>
            <TicketSubSection
              activities={item.activities}
              fullName={fullName}
              t={t}
              className={styles.flex}
            />
          </StyledSubSection>
        </InsightDetail>
      )
    }
    case ActivityType.TicketModified: {
      /**
       * We are showing only below activity key in the UI.
       * All the other activity changes will be ignored completely.
       * Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/84466
       */
      const isValidActivity = _.some(
        item.activities,
        (activity) =>
          activity.key === ActivityKey.Comments ||
          activity.key === ActivityKey.DueDate ||
          activity.key === ActivityKey.Status ||
          activity.key === ActivityKey.AssigneeName ||
          activity.key === ActivityKey.Description
      )

      const insightDescription = _.find(item.activities, {
        key: ActivityKey.Description,
      })
      return (
        isValidActivity && (
          <InsightDetail
            isDefaultExpanded={false}
            headerIcon={<Icon icon="assignment" size={24} tw="mt-[2px]" />}
            headerText={
              <ActivityHeader
                text={
                  <TicketModifiedHeader activities={item.activities} t={t} />
                }
                date={formattedActivityDate}
              />
            }
            sectionHeaderClassName={styles.flex}
          >
            <TicketLink
              fullName={fullName}
              ticketId={item.ticketId}
              ticketSummary={item.ticketSummary}
              className={styles.ticketLink}
              onTicketLinkClick={onTicketLinkClick}
            />

            {item.activities.length > 1 ? (
              <TicketSubSection
                activities={item.activities}
                fullName={fullName}
                t={t}
                className={styles.flex}
              />
            ) : (
              insightDescription?.key === ActivityKey.Description && (
                <TicketTextArea
                  activity={insightDescription}
                  fullName={fullName}
                  t={t}
                  className={styles.flex}
                />
              )
            )}
          </InsightDetail>
        )
      )
    }
    case ActivityType.TicketComment: {
      return (
        <InsightDetail
          isDefaultExpanded={false}
          headerIcon={<Icon icon="assignment" size={24} tw="mt-[2px]" />}
          headerText={
            <ActivityHeader
              text={t('plainText.ticketUpdatedComment')}
              date={formattedActivityDate}
            />
          }
          sectionHeaderClassName={styles.flex}
        >
          <TicketLink
            fullName={fullName}
            ticketId={item.ticketId}
            ticketSummary={item.ticketSummary}
            className={styles.ticketLink}
            onTicketLinkClick={onTicketLinkClick}
          />
        </InsightDetail>
      )
    }
    case ActivityType.TicketAttachment: {
      const headerText =
        item.activities.length > 1
          ? t('plainText.ticketUpdatedMultipleAttachments')
          : t('plainText.ticketUpdatedAttachment')
      return (
        <InsightDetail
          isDefaultExpanded={false}
          headerIcon={<Icon icon="assignment" size={24} tw="mt-[2px]" />}
          headerText={
            <ActivityHeader text={headerText} date={formattedActivityDate} />
          }
          sectionHeaderClassName={styles.flex}
        >
          <TicketLink
            fullName={fullName}
            ticketId={item.ticketId}
            ticketSummary={item.ticketSummary}
            className={styles.ticketLink}
            onTicketLinkClick={onTicketLinkClick}
          />
        </InsightDetail>
      )
    }
    default:
      return null
  }
}

const StyledSubSection = styled.div(({ theme }) => ({
  alignItems: 'center',
  color: theme.color.neutral.fg.default,
}))

const StyledSelect = styled(Select)(({ theme }) => ({
  width: '168px',
  height: '28px',
  margin: `${theme.spacing.s16} 0 0 ${theme.spacing.s16}`,
}))

const StyledFlex = styled.div({
  display: 'flex',
  lineHeight: '20px',
})

const ActivitiesContainer = styled.div`
  container-type: inline-size;
  container-name: insightActivitiesContainer;
`

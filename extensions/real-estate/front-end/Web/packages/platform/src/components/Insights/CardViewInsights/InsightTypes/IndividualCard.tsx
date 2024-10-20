import { ReactNode } from 'react'
import { Icon, Button } from '@willowinc/ui'
import tw, { styled } from 'twin.macro'
import { TFunction } from 'react-i18next'
import { formatDateTime, titleCase } from '@willow/common'
import {
  InsightType,
  InsightPriority,
} from '@willow/common/insights/insights/types'
import {
  PriorityName,
  iconMap,
  TextWithTooltip,
} from '@willow/common/insights/component'

const IndividualCard = ({
  title,
  type,
  insightCount,
  priority,
  lastOccurred,
  impactTitle,
  t,
  language,
  impactScore,
  onClick,
  insightTypeBadge,
}: {
  title: string
  type: InsightType
  insightCount: number
  priority: InsightPriority
  lastOccurred: string
  impactTitle?: string
  t: TFunction
  language: string
  impactScore?: string
  onClick: (e: React.MouseEvent) => void
  insightTypeBadge?: ReactNode
}) => (
  <ContainerButton
    onClick={onClick}
    $linearGradient={iconMap[type]?.linearGradient}
    data-testid="individual-card"
  >
    {[
      <>
        {insightTypeBadge}
        <ClockContainer tw="float-right leading-[20px]">
          <Icon icon="calendar_clock" size={20} />
          {formatDateTime({ value: lastOccurred, language })}
        </ClockContainer>
      </>,
      <StyledTextWithToolTip
        isTitleCase={false}
        text={title}
        isStart
        tooltipWidth="280px"
      />,
      <div tw="flex flex-col w-full">
        <div>
          <SubText tw="float-left">{t('labels.priority')}</SubText>
          {impactScore && <SubText tw="float-right">{impactTitle}</SubText>}
        </div>
        <div>
          <PriorityName tw="float-left" insightPriority={priority} />
          {impactScore && (
            <SemiBoldText tw="float-right">{impactScore}</SemiBoldText>
          )}
        </div>
      </div>,
      <InsightCount>
        {titleCase({
          language,
          text: t('interpolation.viewInsightsCount', {
            count: insightCount,
          }),
        })}
        <Icon icon="arrow_forward" size={20} />
      </InsightCount>,
    ].map((item, index) => (
      <div
        tw="flex w-full justify-between"
        // eslint-disable-next-line react/no-array-index-key
        key={`${title}-${lastOccurred}-${index}`}
      >
        {item}
      </div>
    ))}
  </ContainerButton>
)

const SecondaryButton = (props: React.ComponentPropsWithRef<typeof Button>) => (
  <Button kind="secondary" {...props} />
)

const ContainerButton = styled(SecondaryButton)<{ $linearGradient: string }>(
  ({ theme, $linearGradient }) => ({
    '& .mantine-Button-label': {
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
      justifyContent: 'space-between',
    },
    '& .mantine-Button-inner': {
      margin: '0',
      height: '100%',
      display: 'inline',
    },
    minWidth: '300px',
    width: 'auto',
    height: '220px',
    padding: theme.spacing.s16,
    borderRadius: '4px',
    border: `1px solid ${theme.color.neutral.border.default}`,
    background: $linearGradient,
    backgroundColor: theme.color.neutral.bg.accent.default,
    cursor: 'pointer',

    '&:hover': {
      backgroundColor: theme.color.neutral.bg.accent.hovered,
    },

    '&:focus': {
      border: `1px solid ${theme.color.state.focus.border}`,
      backgroundColor: theme.color.neutral.bg.accent.default,
    },

    '&:active': {
      backgroundColor: theme.color.neutral.bg.accent.activated,
    },
  })
)

const StyledTextWithToolTip = styled(TextWithTooltip)(({ theme }) => ({
  ...theme.font.heading.lg,
  display: '-webkit-box',
  '-webkit-line-clamp': '2',
  '-webkit-box-orient': 'vertical',
  overflow: 'hidden',
  color: theme.color.neutral.fg.default,
  textOverflow: 'ellipsis',
  whiteSpace: 'normal',
  height: '44px',
  textAlign: 'left',
}))

const ClockContainer = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.muted,

  '> span': {
    verticalAlign: 'top',
    paddingRight: theme.spacing.s4,
    color: theme.color.neutral.fg.subtle,
  },
}))

const SubText = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.muted,
  display: 'inline-block',
}))

const SemiBoldText = styled.div(({ theme }) => ({
  ...theme.font.body.lg.semibold,
  color: theme.color.neutral.fg.default,
  display: 'inline-block',
}))

const InsightCount = styled.div(({ theme }) => ({
  borderRadius: '2px',
  border: `1px solid ${theme.color.neutral.border.default}`,
  background: theme.color.neutral.bg.panel.default,
  height: '28px',
  width: '100%',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  color: theme.color.neutral.fg.default,
}))

export default IndividualCard

import { Priority, titleCase, FullSizeContainer } from '@willow/common'
import {
  InsightPriority,
  InsightType,
} from '@willow/common/insights/insights/types'
import {
  IconNew as AssetDetailIcon,
  Checkbox,
  Icon as LegacyIcon,
  Number as NumberComponent,
  Text,
} from '@willow/ui'
import {
  Badge,
  BadgeProps,
  Button,
  Icon,
  IconName,
  IconProps,
} from '@willowinc/ui'
import _ from 'lodash'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useTheme } from 'styled-components'
import { css, styled } from 'twin.macro'
import { getPriorityByRange } from '../costImpacts/utils'
import { getPriorityValue } from '../costImpacts/getInsightPriority'

export const iconMap = {
  fault: {
    icon: 'crisis_alert',
    color: 'yellow',
    value: 'Fault',
    linearGradient:
      'linear-gradient(180deg, rgba(122, 89, 0, 0.20) 0%, rgba(122, 89, 0, 0.00) 48.96%)',
  },
  energy: {
    icon: 'eco',
    color: 'green',
    value: 'Energy',
    linearGradient:
      'linear-gradient(180deg, rgba(34, 108, 35, 0.20) 0%, rgba(34, 108, 35, 0.00) 48.96%)',
  },
  dataQuality: {
    icon: 'query_stats',
    color: 'purple',
    value: 'Data Quality',
    linearGradient:
      'linear-gradient(180deg, rgba(89, 69, 215, 0.20) 0%, rgba(89, 69, 215, 0.00) 48.96%)',
  },
  diagnostic: {
    icon: 'checklist',
    color: 'pink',
    value: 'Diagnostic',
    linearGradient:
      'linear-gradient(180deg, rgba(150, 52, 153, 0.20) 0%, rgba(150, 52, 153, 0.00) 48.96%)',
  },
  commissioning: {
    icon: 'real_estate_agent',
    color: 'blue',
    value: 'Commissioning',
    linearGradient:
      'linear-gradient(180deg, rgba(30, 95, 167, 0.20) 0%, rgba(30, 95, 167, 0.00) 48.96%)',
  },
  predictive: {
    icon: 'build',
    color: 'teal',
    value: 'Predictive',
    linearGradient:
      'linear-gradient(180deg, rgba(35, 106, 81, 0.20) 0%, rgba(38, 103, 112, 0.00) 48.96%)',
  },
  alert: {
    icon: 'assignment_late',
    color: 'orange',
    value: 'Alert',
    linearGradient:
      'linear-gradient(180deg, rgba(152, 71, 23, 0.20) 0%, rgba(152, 71, 23, 0.00) 48.96%)',
  },
  note: {
    icon: 'assignment',
    color: 'cyan',
    value: 'Note',
    linearGradient:
      'linear-gradient(180deg, rgba(38, 103, 112, 0.20) 0%, rgba(38, 103, 112, 0.00) 48.96%)',
  },
  alarm: {
    icon: 'e911_emergency',
    color: 'red',
    value: 'Alarm',
    linearGradient:
      'linear-gradient(180deg, rgba(176, 43, 51, 0.20) 0%, rgba(176, 43, 51, 0.00) 48.96%)',
  },
  comfort: {
    icon: 'person_celebrate',
    color: 'cyan',
    value: 'Comfort',
    linearGradient:
      'linear-gradient(180deg, rgba(38, 103, 112, 0.20) 0%, rgba(38, 103, 112, 0.00) 48.96%)',
  },
  goldenStandard: {
    value: 'Golden Standard',
    linearGradient: undefined,
  },
  infrastructure: {
    value: 'Infrastructure',
    linearGradient: undefined,
  },
  integrityKpi: {
    value: 'Integrity KPI',
    linearGradient: undefined,
  },
  energyKpi: {
    value: 'Energy KPI',
    linearGradient: undefined,
  },
  edgeDevice: {
    value: 'Edge Device',
    linearGradient: undefined,
  },
  wellness: {
    value: 'Wellness',
    linearGradient: undefined,
  },
  calibration: {
    value: 'Calibration',
    linearGradient: undefined,
  },
  multiple: {
    value: 'Multiple',
    linearGradient:
      'linear-gradient(180deg, rgba(94, 94, 94, 0.20) 0%, rgba(94, 94, 94, 0.00) 48.96%);',
  },
}

/**
 * This component is used to display the badge for insight type,
 * we do not translate these values as they bear some branding value
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/89416
 */
export const InsightTypeBadge = ({
  type,
  badgeSize = 'sm',
  iconSize = 16,
}: {
  type: InsightType
  badgeSize?: BadgeProps['size']
  iconSize?: IconProps['size']
}) => {
  const iconColor = iconMap[type]

  return (
    <Badge
      size={badgeSize}
      variant="subtle"
      color={iconColor?.color}
      {...(iconColor?.icon
        ? {
            prefix: <Icon size={iconSize} icon={iconColor.icon} />,
          }
        : {})}
    >
      {iconColor?.value ?? ''}
    </Badge>
  )
}

/**
 * This component is used to display the tooltip on hover
 * of the Checkbox if it overflows its container
 */
export const CheckboxWithTooltip = ({
  value,
  tooltipText,
  tooltipWidth = '180px',
  tooltipTime = '500',
  onChange,
  isTitleCase = true,
  ...rest
}: {
  value: boolean
  tooltipText: string
  tooltipWidth?: string
  tooltipTime?: string
  onChange?: () => void
  isTitleCase?: boolean
}) => (
  <StyledCheckbox value={value} onChange={onChange} {...rest}>
    <TextWithTooltip
      text={tooltipText}
      tooltipWidth={tooltipWidth}
      tooltipTime={tooltipTime}
      isTitleCase={isTitleCase}
    />
  </StyledCheckbox>
)

/**
 * This component is used to display the tooltip on hover
 * of the text if it overflows its container
 *
 * @deprecated
 * If you want to use this inside a text cell in `DataGrid`, use `valueGetter`
 * to define column value instead, which will apply the default HTML title tooltip
 * for the text if it overflows its container.
 *
 * If you need to apply a tooltip outside a `DataGrid`, use
 * `TooltipWhenTruncated` component which uses the `@willowinc/ui`
 * `Tooltip` component underneath.
 */
export const TextWithTooltip = ({
  text,
  tooltipWidth,
  tooltipTime = '500',
  className = undefined,
  isTitleCase = true,
  isStart = false,
}: {
  text: string
  tooltipWidth?: string
  tooltipTime?: string
  className?: string
  isTitleCase?: boolean
  isStart?: boolean
}) => {
  const theme = useTheme()
  const {
    i18n: { language },
  } = useTranslation()
  const textElementRef = useRef<HTMLSpanElement | null>(null)
  const [showTooltip, setShowTooltip] = useState(false)

  const handleMouseEnter = () => {
    const isOverflowing =
      (textElementRef.current?.scrollWidth ?? 0) >
        (textElementRef.current?.clientWidth ?? 0) ||
      (textElementRef.current?.scrollHeight ?? 0) >
        (textElementRef.current?.clientHeight ?? 0)

    setShowTooltip(isOverflowing)
  }

  return (
    <TooltipContainer
      onMouseEnter={handleMouseEnter}
      data-tooltip={showTooltip ? text : undefined}
      data-tooltip-position="top"
      data-tooltip-width={tooltipWidth ?? undefined}
      data-tooltip-time={tooltipTime}
      data-tooltip-z-index={theme.zIndex.popover}
      $isStart={isStart}
    >
      <FormattedText className={className} ref={textElementRef}>
        {isTitleCase ? titleCase({ text, language }) : text}
      </FormattedText>
    </TooltipContainer>
  )
}

const TooltipContainer = styled.span<{ $isStart: boolean }>(({ $isStart }) => ({
  height: '100%',
  width: '100%',
  display: 'flex',
  alignItems: $isStart ? 'flex-start' : 'center',
}))

/**
 * A component to display the empty state of InsightDetail component
 * When no content is present, it shows the empty state message in UI
 */
export const InsightDetailEmptyState = ({
  heading,
  subHeading,
}: {
  heading: string
  subHeading: string
}) => (
  <FullSizeContainer
    css={`
      display: flex;
      flex-direction: column;
    `}
  >
    <Heading> {heading} </Heading>
    <SubHeading>{subHeading}</SubHeading>
  </FullSizeContainer>
)

const Heading = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.heading.md,
}))
const SubHeading = styled.div(({ theme }) => ({
  marginTop: theme.spacing.s16,
  color: theme.color.intent.secondary.fg.default,
}))

/**
 * This component formats the priority value and if
 * the priority value is not available, it returns '--'
 */
export const PriorityValue = ({ value }: { value?: string }) =>
  value != null && value !== '--' ? (
    <NumberComponent value={value} format="0.[00]" />
  ) : (
    '--'
  )

const FormattedText = styled(Text)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
  display: 'block',
}))

const StyledCheckbox = styled(Checkbox)({
  paddingRight: '4px',

  '&&&': {
    maxWidth: '280px',
  },
})

interface PriorityBadgeProps extends BadgeProps {
  priority: Priority | null
}
/**
 * Badge component returns the priority name with color based on
 * priority.
 */
export const PriorityBadge = ({ priority, ...rest }: PriorityBadgeProps) => {
  const { t } = useTranslation()

  return (
    <Badge
      size="md"
      variant="muted"
      color={priority?.color ?? 'gray'}
      css={{
        textTransform: 'capitalize',
      }}
      {...rest}
    >
      {priority?.name ? t(`plainText.${priority?.name.toLowerCase()}`) : '--'}
    </Badge>
  )
}

/**
 * This component returns the priority name with color based on
 * impactScorePriority value and insightPriority value
 *
 * @deprecated
 * use PriorityBadge instead
 */
export const PriorityName = ({
  impactScorePriority,
  insightPriority,
  ...rest
}: {
  impactScorePriority?: number | string
  insightPriority: InsightPriority
} & Omit<PriorityBadgeProps, 'priority'>) => {
  const priority = getPriorityByRange(
    getPriorityValue({ impactScorePriority, insightPriority })
  )

  return <PriorityBadge priority={priority} {...rest} />
}

/**
 * A component to display the insight details in the insight drawer;
 * it contains the header section which includes an icon, header text and a chevron icon;
 * and the content section which includes some information about the insight
 */
export const InsightDetail = ({
  headerIcon,
  headerText,
  children,
  isDefaultExpanded = true,
  sectionHeaderClassName,
}: {
  headerIcon?: string | React.ReactElement
  headerText: string | React.ReactElement
  children?: React.ReactNode
  isDefaultExpanded?: boolean
  sectionHeaderClassName?: string
}) => {
  const [isExpanded, setIsExpanded] = useState(isDefaultExpanded)
  const showCollapsablePanel = (isExpanded && children) as boolean

  return (
    <SectionContainer>
      <SectionItemContainer
        $borderBottom={showCollapsablePanel}
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <SectionContents tw="items-center">
          {typeof headerIcon === 'string' ? (
            <AssetDetailIcon icon={headerIcon} />
          ) : (
            headerIcon
          )}
          <SectionHeader className={sectionHeaderClassName} tw="truncate">
            {headerText}
          </SectionHeader>
          {children ? (
            <Button kind="secondary" background="transparent">
              <ChevronIcon
                tw="pointer-events-none"
                icon="chevron"
                $isExpanded={isExpanded}
              />
            </Button>
          ) : (
            <span tw="w-[34px]" />
          )}
        </SectionContents>
      </SectionItemContainer>
      {showCollapsablePanel && (
        <SectionContents tw="flex-col max-h-[232px] overflow-y-auto">
          {children}
        </SectionContents>
      )}
    </SectionContainer>
  )
}

export const Container = styled.div<{ $hidePaddingBottom?: boolean }>(
  ({ theme, $hidePaddingBottom }) => ({
    position: 'relative',
    padding: `${theme.spacing.s24} ${theme.spacing.s32}`,
    paddingBottom: $hidePaddingBottom ? '0px' : `${theme.spacing.s24}`,
    gap: theme.spacing.s24,
    display: 'flex',
    flexDirection: 'column',
  })
)

const SectionContainer = styled.div(({ theme }) => ({
  borderRadius: theme.spacing.s4,
  background: theme.color.neutral.bg.accent.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
}))

const SectionItemContainer = styled.div<{ $borderBottom: boolean }>(
  ({ theme, $borderBottom }) => ({
    borderBottom: $borderBottom
      ? `1px solid ${theme.color.neutral.border.default}`
      : 'none',
    cursor: 'pointer',
  })
)

const SectionContents = styled.div(({ theme }) => ({
  gap: theme.spacing.s16,
  display: 'flex',
  padding: `${theme.spacing.s12} ${theme.spacing.s16}`,
}))

const SectionHeader = styled.div(({ theme }) => ({
  display: 'flex',
  color: theme.color.neutral.fg.default,
  ...theme.font.heading.lg,
  textTransform: 'capitalize',
  width: '100%',
  justifyContent: 'space-between',
}))

const ChevronIcon = styled(LegacyIcon)<{ $isExpanded: boolean }>(
  ({ $isExpanded }) => ({
    transform: $isExpanded ? 'rotate(-180deg)' : undefined,
    transition: 'var(--transition-out)',
  })
)

/**
 * bulk action icon button for insights table
 * could be to "delete", "ignore", "report" and "resolve"
 * one or multiple insights
 */
export const ActionIcon = styled(Icon)<{
  $isRed?: boolean
  $enabled?: boolean
  fontSize?: string
  marginBottom?: string
}>(
  ({
    theme,
    $isRed = false,
    $enabled = true,
    fontSize = '20px',
    marginBottom,
  }) => {
    const getColors = (enabled: boolean, isRed: boolean) => {
      if (!enabled) {
        return {
          color: theme.color.state.disabled.fg,
          hoveredColor: theme.color.state.disabled.fg,
        }
      }

      return isRed
        ? {
            color: theme.color.intent.negative.fg.default,
            hoveredColor: theme.color.intent.negative.fg.hovered,
          }
        : {
            color: theme.color.neutral.fg.default,
            hoveredColor: theme.color.neutral.fg.highlight,
          }
    }

    const { color, hoveredColor } = getColors($enabled, $isRed)

    return {
      marginBottom,
      cursor: $enabled ? 'pointer' : 'default',
      marginRight: '1.125rem',
      // to override the default color of the icon
      // since "delete" icon is red, and all other icons are gray
      '&&&': {
        fontSize,
        color,

        '&:hover': {
          color: hoveredColor,
        },
      },
    }
  }
)

/**
 * This component handles the display of activity icons and counts
 * If the activityCount is greater than 0, it shows both the icon and count
 * If the isIcon is true, it displays only the icon without the count
 */
export const ActivityCount = ({
  icon,
  tooltipText,
  activityCount,
  filled = true,
  isVisible = true,
  isIcon = false,
}: {
  icon: IconName
  tooltipText: string
  activityCount?: number
  filled?: boolean
  isVisible?: boolean
  isIcon?: boolean
}) =>
  isVisible && activityCount != null && activityCount > 0 ? (
    <>
      <Icon
        css={css(({ theme }) => ({
          margin: `0 ${theme.spacing.s4}`,
          fontSize: theme.font.heading.xl.fontSize,
          '&&&': {
            color: theme.color.neutral.fg.default,
          },
        }))}
        icon={icon}
        data-tooltip={_.startCase(tooltipText)}
        data-tooltip-position="top"
        filled={filled}
      />
      {!isIcon && (
        <div
          css={css(({ theme }) => ({
            marginLeft: theme.spacing.s2,
            marginRight: theme.spacing.s8,
            display: 'flex',
            witeSpace: 'nowrap',
            height: theme.spacing.s16,
            ...theme.font.body.xs.regular,
          }))}
        >{`x ${activityCount}`}</div>
      )}
    </>
  ) : null

export const PriorityBar = styled.div<{
  priorityLevel: InsightPriority | undefined
}>(({ priorityLevel }) => ({
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'center',
  '&::before': {
    content: '""',
    backgroundColor: priorityLevel
      ? priorityColors[priorityLevel]
      : 'transparent' /* don't show if no valid priority */,
    width: '4px',
    height: '48px',
    transform: 'translateX(-1px)',
  },
}))

const priorityColors = {
  1: '#FC2D3B', // urgent priority
  2: '#FF6200', // high priority
  3: '#FFC11A', // medium priority
  4: '#417CBF', // low priority
}

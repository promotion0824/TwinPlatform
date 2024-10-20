import { TFunction, useTranslation } from 'react-i18next'
import tw, { styled } from 'twin.macro'
import { useDateTime } from '../../../hooks'
import Button from '../../Button/Button'
import Text from '../../Text/Text'

const rangeOptions: {
  [key: string]: {
    dataSegment: string
    translate: {
      title: Parameters<TFunction>
      tooltip: Parameters<TFunction>
    }
    days?: number
  }
} = {
  '24H': {
    dataSegment: 'Last 24 Hours',
    translate: {
      title: ['plainText.24H'],
      tooltip: [
        'interpolation.numberOfHours',
        undefined,
        { replace: { num: 24 } },
      ],
    },
    days: -1,
  },
  '48H': {
    dataSegment: 'Last 48 Hours',
    translate: {
      title: ['plainText.48H'],
      tooltip: [
        'interpolation.numberOfHours',
        undefined,
        { replace: { num: 48 } },
      ],
    },
    days: -2,
  },
  '7D': {
    dataSegment: 'Last 7 Days',
    translate: {
      title: ['plainText.7D'],
      tooltip: [
        'interpolation.numberOfDays',
        undefined,
        { replace: { num: 7 } },
      ],
    },
    days: -7,
  },
  '1M': {
    dataSegment: 'Last 30 Days',
    translate: {
      title: ['plainText.oneMonthShort'],
      tooltip: ['plainText.oneMonthCap'],
    },
    days: -30,
  },
  thisMonth: {
    dataSegment: 'This Month',
    translate: {
      title: ['plainText.thisMonths'],
      tooltip: ['plainText.thisMonths'],
    },
  },
  prevMonth: {
    dataSegment: 'Last Month',
    translate: {
      title: ['plainText.prevMonth'],
      tooltip: ['plainText.prevMonth'],
    },
  },
  '3M': {
    dataSegment: 'Last 3 months',
    translate: {
      title: ['plainText.threeMonthsShort'],
      tooltip: [
        'interpolation.numberOfMonths',
        undefined,
        { replace: { num: 3 } },
      ],
    },
    days: -90,
  },
  '6M': {
    dataSegment: 'Last 6 months',
    translate: {
      title: ['plainText.sixMonthsShort'],
      tooltip: [
        'interpolation.numberOfMonths',
        undefined,
        { replace: { num: 6 } },
      ],
    },
    days: -180,
  },
  '1Y': {
    dataSegment: 'Last year',
    translate: {
      title: ['plainText.oneYearShort'],
      tooltip: [
        'interpolation.numberOfYears_one',
        undefined,
        { replace: { count: 1 } },
      ],
    },
    days: -365,
  },
}

export type QuickRangeOption = keyof typeof rangeOptions

export type DateTimeRange = [from: string, to: string]

const getMonthRange = (dateTime): DateTimeRange => [
  dateTime.startOfMonth().startOfDay().format(),
  dateTime.endOfMonth().endOfDay().format(),
]

type QuickRangeOptionsProps = {
  options: QuickRangeOption[]
  onSelect: (selected: QuickRangeOption, dateTimeRange: DateTimeRange) => void
  selected?: QuickRangeOption
  className?: string
  'data-segment'?: string
  timezone?: string
}

/**
 * Helper to get the date time range based on current DateTime, for a
 * specified quick range option.
 */
export const getDateTimeRange = (
  now,
  quickRange: QuickRangeOption
): DateTimeRange => {
  switch (quickRange) {
    case 'thisMonth':
      return getMonthRange(now)
    case 'prevMonth':
      return getMonthRange(now.addMonths(-1))
    default:
      return [
        now.addDays(rangeOptions[quickRange]?.days).format(),
        now.format(),
      ]
  }
}

/**
 * quick options buttons under calendar, reference:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/60993
 * it is the group of buttons under Calender component that can quickly
 * toggle between different time/date range
 */
export default function QuickRangeOptions({
  options,
  onSelect,
  selected,
  className,
  timezone,
  'data-segment': dataSegment,
}: QuickRangeOptionsProps) {
  const { t } = useTranslation()
  const dateTime = useDateTime()

  return (
    <QuickOptionsContainer className={className}>
      {options.map((optionKey) => {
        const rangeOption = rangeOptions[optionKey]

        const { title, tooltip } = rangeOption.translate

        return (
          <QuickButtonFlex
            key={optionKey}
            onClick={() =>
              onSelect(
                optionKey,
                getDateTimeRange(dateTime.now(timezone), optionKey)
              )
            }
            selected={selected === optionKey}
            data-segment={dataSegment}
            data-segment-props={
              dataSegment
                ? JSON.stringify({ option: rangeOption.dataSegment })
                : undefined
            }
            title={tooltip.length > 0 ? t(...tooltip) : undefined}
          >
            <Text size="small" weight="medium">
              {t(...title)}
            </Text>
          </QuickButtonFlex>
        )
      })}
    </QuickOptionsContainer>
  )
}

const QuickOptionsContainer = styled(tw.div`flex`)({
  justifyContent: 'space-between',
  height: 30,
})

const QuickButtonFlex = styled(Button)<{ selected: boolean }>(
  ({ selected }) => ({
    justifyContent: 'center',
    border: '1px solid #3E3E3E',
    borderRadius: '2px',
    minWidth: 33,
    height: 30,
    padding: '0 6px',
    flex: 1,
    flexBasis: 'auto',
    flexShrink: 0,
    backgroundColor: selected ? 'var(--disabled)' : '',

    '&:not(:first-child)': {
      marginLeft: 8,
    },
  })
)

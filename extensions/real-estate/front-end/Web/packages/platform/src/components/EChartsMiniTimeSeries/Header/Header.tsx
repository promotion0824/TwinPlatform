/* eslint-disable react/require-default-props */
import { useState } from 'react'
import { DatePicker } from '@willow/ui'
import { Button } from '@willowinc/ui'
import { css } from 'twin.macro'
import { QuickRangeOption } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { TimeSeriesTwinInfo } from '@willow/common/insights/insights/types'
import { useTranslation } from 'react-i18next'
import { ResizeObserverContainer } from '@willow/common'
import Equipments from '../Equipments/Equipments'
import ExportButton from '../ExportButton/ExportButton'
import TimeZoneSelect, {
  TimeZoneOption,
} from '../../TimeZoneSelect/TimeZoneSelect'

const quickRangeOptions: QuickRangeOption[] = [
  '24H',
  '48H',
  '7D',
  'thisMonth',
  'prevMonth',
  '3M',
]

type HeaderProps = {
  quickRange?: QuickRangeOption
  onQuickRangeChange: (quickRange: QuickRangeOption) => void
  times: [from: string, until: string]
  onTimesChange: (times: [string, string], isCustom: boolean) => void
  siteId: string
  assetId: string
  equipmentDisabled: boolean
  onTimeSeriesClick: () => void
  equipmentName: string
  timeZoneOption?: TimeZoneOption['value']
  timeZone?: string
  onTimeZoneChange: (
    value: TimeZoneOption['value'],
    timeZoneOption: TimeZoneOption
  ) => void
  twinInfo?: TimeSeriesTwinInfo
}

const Header = ({
  quickRange,
  onQuickRangeChange,
  times,
  onTimesChange,
  siteId,
  assetId,
  equipmentDisabled = false,
  onTimeSeriesClick,
  equipmentName,
  timeZoneOption,
  timeZone,
  onTimeZoneChange,
  twinInfo,
}: HeaderProps) => {
  const { t } = useTranslation()
  const [isFlxColumn, setIsFlxColumn] = useState(false)

  return (
    <ResizeObserverContainer
      onContainerWidthChange={(width) => setIsFlxColumn(width < 600)}
    >
      <div
        data-testid="mini-time-series-header"
        css={css(
          ({ theme }) => `
        display: flex;
        width: 100%;
        padding: ${theme.spacing.s16};
        padding-bottom: ${theme.spacing.s4};
        gap: ${theme.spacing.s8};
        ${isFlxColumn ? 'flex-direction: column;' : ''}
        justify-content: flex-end;
        & > *:first-child {
          margin-right: auto;
          flex-shrink: 1;
          flex-wrap: nowrap;
          overflow: hidden;
          min-width: 0px;
        }
        & > *:nth-child(2) {
          white-space: nowrap;
        }
      `
        )}
      >
        <DatePicker
          type="date-time-range"
          quickRangeOptions={quickRangeOptions}
          selectedQuickRange={quickRange}
          onSelectQuickRange={onQuickRangeChange}
          value={times}
          onChange={onTimesChange}
          // The backend does not allow us to retrieve data over a range of
          // more than 371 days (which equals 53 weeks).
          maxDays={371}
          data-segment="Mini Time Series Calendar Expanded"
          timezone={timeZone}
          timezoneSelector={
            <TimeZoneSelect
              value={timeZoneOption}
              onChange={onTimeZoneChange}
              siteIds={[siteId]}
            />
          }
        />

        {!equipmentDisabled && (
          <Equipments siteId={siteId} assetId={assetId} twinInfo={twinInfo} />
        )}

        <Button
          css={css(
            ({ theme }) => `
          height: 30px;
          line-height: 22px;
          color: ${theme.color.neutral.fg.default};
          background-color: ${theme.color.neutral.bg.accent.activated};
          &:hover {
            color: ${theme.color.neutral.fg.highlight};
            background-color: ${theme.color.neutral.bg.accent.activated};
          }
        `
          )}
          variant="secondary"
          background="transparent"
          onClick={onTimeSeriesClick}
          data-segment="Go To TimeSeries Clicked"
          data-testid="go-to-timeSeries-button"
          data-segment-props={JSON.stringify({
            siteId,
            equipment_name: equipmentName,
          })}
        >
          {t('labels.goToTimeSeries')}
        </Button>

        <ExportButton timeZoneId={timeZoneOption?.timeZoneId} />
      </div>
    </ResizeObserverContainer>
  )
}

export default Header

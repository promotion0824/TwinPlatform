import { DatePicker, Flex, useScopeSelector } from '@willow/ui'
import { Button, Icon, useDisclosure } from '@willowinc/ui'
import TimeZoneSelect from 'components/TimeZoneSelect/TimeZoneSelect.tsx'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { useTimeSeries } from '../../components/TimeSeries/TimeSeriesContext'
import { useSite } from '../../providers'
import ExportButton from './ExportButton/ExportButton'
import FavoritesDrawer from './Favorites/FavoritesDrawer'
import SaveFavoriteModal from './Favorites/SaveFavoriteModal'
import styles from './TimeSeriesHeader.css'

const quickRangeOptions = ['24H', '48H', '7D', 'thisMonth', 'prevMonth', '3M']

const DatePickerContainer = styled.div({
  maxWidth: '100%',
})

export default function TimeSeriesHeader({
  quickRange,
  onQuickRangeChange,
  times,
  onTimesChange,
  timeZoneOption,
  timeZone,
  onTimeZoneChange,
}) {
  const timeSeries = useTimeSeries()

  const siteIds = useMemo(
    () => timeSeries.assets.map((asset) => asset.siteId),
    [timeSeries.assets]
  )

  return (
    <DatePickerContainer>
      <DatePicker
        type="date-time-range"
        className={
          quickRange
            ? styles.headerButton
            : `${styles.headerButton} ${styles.active}`
        }
        quickRangeOptions={quickRangeOptions}
        selectedQuickRange={quickRange}
        onSelectQuickRange={onQuickRangeChange}
        value={times}
        onChange={(pickedTimes, isCustomRange) => {
          onTimesChange(
            pickedTimes,
            isCustomRange /* Send analytics, only when date range custom */
          )
        }}
        // The backend does not allow us to retrieve data over a range of
        // more than 371 days (which equals 53 weeks).
        maxDays={371}
        timezone={timeZone}
        timezoneSelector={
          <TimeZoneSelect
            value={timeZoneOption}
            onChange={onTimeZoneChange}
            siteIds={siteIds}
          />
        }
        data-segment="Time Series Calendar Expanded"
      />
    </DatePickerContainer>
  )
}

export const TimeSeriesButtonControls = ({ timeZoneOption }) => {
  const scopeSelector = useScopeSelector()
  const site = useSite()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()
  const { isScopeSelectorEnabled } = useScopeSelector()

  // This will fallback to the useSite siteId if it can't get a siteId from Scope Selector.
  // This is done for backwards compatibility, and will be reworked when scope compatible
  // API routes are added in 116005/116007.
  const siteId =
    (scopeSelector.isScopeSelectorEnabled &&
      !scopeSelector.location?.children?.length &&
      scopeSelector.location?.twin.siteId) ||
    site.id

  const [
    favoritesDrawerOpened,
    { close: closeFavoritesDrawer, open: openFavoritesDrawer },
  ] = useDisclosure()

  const [saveModalOpened, { close: closeSaveModal, open: openSaveModal }] =
    useDisclosure()

  return (
    <>
      <Flex horizontal align="middle right" padding="0" size="medium">
        <Button
          kind="secondary"
          prefix={<Icon icon="view_list" />}
          onClick={openFavoritesDrawer}
          data-segment="Time Series Favorites Opened"
        >
          {t('headers.favorites')}
        </Button>
        {timeSeries.assets.length > 0 && (
          <Button
            prefix={<Icon icon="save" />}
            data-segment="Time Series Save Favorite Button"
            onClick={openSaveModal}
          >
            {t('plainText.saveFavorites')}
          </Button>
        )}
        <ExportButton timeZoneId={timeZoneOption?.timeZoneId} />
      </Flex>

      {(!!siteId || isScopeSelectorEnabled) && (
        <>
          <FavoritesDrawer
            onClose={closeFavoritesDrawer}
            opened={favoritesDrawerOpened}
            siteId={siteId}
          />

          <SaveFavoriteModal
            onClose={closeSaveModal}
            opened={saveModalOpened}
            siteId={siteId}
          />
        </>
      )}
    </>
  )
}

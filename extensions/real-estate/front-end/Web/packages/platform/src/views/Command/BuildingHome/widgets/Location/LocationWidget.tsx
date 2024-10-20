import { useUser } from '@willow/ui'
import { Card, IconButton, Stack } from '@willowinc/ui'
import { css } from 'styled-components'
import {
  Forecast,
  ForecastTile,
} from '../../../../../components/LocationHome/ForecastTile/ForecastTile'
import { LocationDataTile } from '../../../../../components/LocationHome/LocationDataTile/LocationDataTile'
import { PerformanceTile } from '../../../../../components/LocationHome/PerformanceTile/PerformanceTile'
import EditingModeOverlay from '../../../../../components/LocationHome/WidgetCard/EditingModeOverlay'
import { useSite } from '../../../../../providers'
import {
  useBuildingHomeSlice,
  WidgetId,
} from '../../../../../store/buildingHomeSlice'
import { User } from '../../../../Layout/Layout/Header/UserMenu/types'
import useCancelableEditModal from '../useCancelableEditModal'
import LocationWidgetEditModal from './LocationWidgetEditModal'
import LocationWidgetHeader from './LocationWidgetHeader'

const LocationWidget = () => {
  const site = useSite()
  const user: User = useUser()

  const { isEditingMode } = useBuildingHomeSlice()

  const {
    widgetConfig: locationWidgetFeatures,
    setWidgetConfig,
    editModalOpened,
    onOpenEditModal,
    onCloseEditModal,
    onCancelEdit,
    onSaveEdit,
  } = useCancelableEditModal(WidgetId.Location)

  const showOverallPerformance =
    locationWidgetFeatures?.showOverallPerformance ?? false
  const setShowOverallPerformance = (value: boolean) => {
    setWidgetConfig({
      ...locationWidgetFeatures,
      showOverallPerformance: value,
    })
  }

  // TODO: Test data for BuildingHomeDashboard
  const averageScore = 95
  const performanceScores = [
    { label: 'Feb 08', value: 100 },
    { label: 'Feb 09', value: 70 },
    { label: 'Feb 10', value: 45 },
    { label: 'Feb 11', value: 90 },
    { label: 'Feb 12', value: 95 },
    { label: 'Feb 13', value: 100 },
    { label: 'Feb 14', value: 65 },
    { label: 'Feb 15', value: 90 },
    { label: 'Feb 16', value: 85 },
    { label: 'Feb 17', value: 35 },
    { label: 'Feb 18', value: 100 },
    { label: 'Feb 19', value: 95 },
    { label: 'Feb 20', value: 90 },
    { label: 'Feb 21', value: 85 },
    { label: 'Feb 22', value: 95 },
    { label: 'Feb 23', value: 100 },
    { label: 'Feb 24', value: 68 },
    { label: 'Feb 25', value: 62 },
    { label: 'Feb 26', value: 60 },
    { label: 'Feb 27', value: 56 },
    { label: 'Feb 28', value: 100 },
    { label: 'Mar 01', value: 95 },
    { label: 'Mar 02', value: 90 },
    { label: 'Mar 03', value: 85 },
    { label: 'Mar 04', value: 95 },
    { label: 'Mar 05', value: 48 },
    { label: 'Mar 06', value: 44 },
    { label: 'Mar 07', value: 72 },
  ]
  const forecast: Forecast = [
    {
      code: 800,
      icon: 'c01d',
      temperature: 13.3,
    },
    {
      code: 700,
      icon: 'a01d',
      temperature: 8.3,
    },
    {
      code: 800,
      icon: 'c01d',
      temperature: 13.3,
    },
    {
      code: 300,
      icon: 'd01d',
      temperature: 12.2,
    },
    {
      code: 802,
      icon: 'c02d',
      temperature: 8.3,
    },
  ]
  // In the future this will likely be added to Site itself.
  const yearOpened = 2019

  return (
    <Card
      background="panel"
      css={css(({ theme }) => ({
        borderRadius: theme.radius.r4,
        height: 'auto',
        width: '320px',
      }))}
    >
      <LocationWidgetEditModal
        opened={editModalOpened}
        onClose={onCloseEditModal}
        onCancel={onCancelEdit}
        onSave={onSaveEdit}
        onShowOverallPerformanceChange={setShowOverallPerformance}
        showOverallPerformance={showOverallPerformance}
      />

      {isEditingMode && (
        <IconButton
          background="transparent"
          css={css(({ theme }) => ({
            float: 'inline-end',
            zIndex: theme.zIndex.overlay,
          }))}
          mt="s12"
          mr="s12"
          icon="edit"
          kind="secondary"
          onClick={onOpenEditModal}
        />
      )}

      <EditingModeOverlay $isEditingMode={isEditingMode}>
        <LocationWidgetHeader
          isEditingMode={isEditingMode}
          onEditClick={onOpenEditModal}
          site={site}
        />

        <Stack gap="s12" pt="s12" pb="s12" pl="s16" pr="s16">
          {showOverallPerformance &&
            site.status.toLowerCase() === 'operations' &&
            averageScore !== undefined &&
            performanceScores && (
              <PerformanceTile
                averageScore={averageScore}
                intentThresholds={{
                  noticeThreshold: 50,
                  positiveThreshold: 75,
                }}
                performanceScores={performanceScores}
              />
            )}

          <ForecastTile
            forecast={forecast}
            temperatureUnit={user.options.temperatureUnit}
          />

          <LocationDataTile
            area={site.area}
            location={`${site.suburb}, ${site.state}`}
            status={site.status}
            timeZone={site.timeZone}
            yearOpened={yearOpened}
          />
        </Stack>
      </EditingModeOverlay>
    </Card>
  )
}

export default LocationWidget

import {
  api,
  NotFound,
  useAnalytics,
  useDateTime,
  useScopeSelector,
  useSnackbar,
  useUser,
} from '@willow/ui'
import { getDateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { Button, DataGrid, Drawer, Icon, IconButton, Menu } from '@willowinc/ui'
import { xor } from 'lodash'
import { TFunction, useTranslation } from 'react-i18next'
import { useQueryClient, UseQueryResult } from 'react-query'
import styled from 'styled-components'
import { useTimeSeries } from '../../../components/TimeSeries/TimeSeriesContext'
import useGetTimeSeriesSiteFavorites from '../../../hooks/TimeSeries/useGetTimeSeriesSiteFavorites'
import { useSites } from '../../../providers'
import type { TimeSeries, TimeSeriesFavorite } from '../types'
import {
  updateTimeSeriesScopeFavorites,
  useTimeSeriesScopeFavorites,
} from './useGetTimeSeriesScopeFavorites'

type FavoriteKind = 'personal' | 'site' | 'scope'

const TrackingTextByFavoriteKind = {
  personal: 'My',
  scope: 'Shared',
  site: 'Site',
}

const DrawerBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s12,
  padding: theme.spacing.s16,
}))

const Heading = styled.div(({ theme }) => ({
  ...theme.font.heading.group,
  textTransform: 'uppercase',
}))

const LoadButton = styled(Button)({
  width: '60px',
})

/**
 * Check if the duration of the favorite's DateTime range is equal to the duration in TimeSeries
 * state with the following conditions:
 * - If quick range is present, then we check whether the quick range value is equal.
 * - If quick range is not specified, check if the difference between the favorite timeDiffs
 * matches the difference in milliseconds for the Date range in TimeSeries state.
 */
const isDurationEqual = (
  dateTime: ReturnType<typeof useDateTime>,
  favorite: TimeSeriesFavorite,
  timeSeries: TimeSeries
) =>
  favorite.quickSelectTimeRange || timeSeries.quickRange
    ? favorite.quickSelectTimeRange === timeSeries.quickRange
    : favorite.timeDiffs[0] - favorite.timeDiffs[1] ===
      dateTime(timeSeries.state.times[1]).differenceInMilliseconds(
        timeSeries.state.times[0]
      )

/**
 * A favorite is loaded if:
 * - The selected points are the same as the TimeSeries' state, and
 * - The selected equipments/twins are the same as the TimeSeries' state, and
 * - The duration saved in the favorite is the same as the duration in TimeSeries' state
 */
function isFavoriteLoaded(
  dateTime: ReturnType<typeof useDateTime>,
  favorite: TimeSeriesFavorite,
  timeSeries: TimeSeries
) {
  return (
    !xor(favorite.sitePointIds, timeSeries.state.sitePointIds).length &&
    !xor(favorite.siteEquipmentIds, timeSeries.state.siteEquipmentIds).length &&
    isDurationEqual(dateTime, favorite, timeSeries)
  )
}

function FavoritesDataGrid({
  dateTime,
  kind,
  onLoadFavorite,
  onRemoveFavorite,
  rows,
  t,
  timeSeries,
}: {
  dateTime: ReturnType<typeof useDateTime>
  kind: FavoriteKind
  onLoadFavorite: (favorite: TimeSeriesFavorite, kind: FavoriteKind) => void
  onRemoveFavorite: (favorite: TimeSeriesFavorite, kind: FavoriteKind) => void
  rows: TimeSeriesFavorite[]
  t: TFunction
  timeSeries: TimeSeries
}) {
  return (
    <DataGrid
      columns={[
        {
          field: 'name',
          flex: 1,
          headerName: t('labels.name'),
        },
        {
          disableReorder: true,
          field: 'load',
          headerName: '',
          renderCell: ({ row }) => {
            const favorite = row as TimeSeriesFavorite
            const isLoaded = isFavoriteLoaded(dateTime, favorite, timeSeries)

            return (
              <LoadButton
                kind={isLoaded ? 'primary' : 'secondary'}
                onClick={() => onLoadFavorite(favorite, kind)}
              >
                {isLoaded ? t('plainText.loaded') : t('plainText.load')}
              </LoadButton>
            )
          },
          width: 60 + 12 + 12, // Button width + padding
        },
        {
          disableReorder: true,
          field: 'menu',
          headerName: '',
          renderCell: ({ row }) => {
            const favorite = row as TimeSeriesFavorite

            return (
              <Menu>
                <Menu.Target>
                  <IconButton
                    background="transparent"
                    icon="more_vert"
                    kind="secondary"
                  />
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item
                    onClick={() => onRemoveFavorite(favorite, kind)}
                    suffix={<Icon icon="delete" />}
                  >
                    {t('plainText.removeFromFavs')}
                  </Menu.Item>
                </Menu.Dropdown>
              </Menu>
            )
          },
          width: 28 + 12 + 12, // Button width + padding
        },
      ]}
      getRowId={(row: TimeSeriesFavorite) => row.name}
      hideFooter
      initialState={{
        sorting: { sortModel: [{ field: 'name', sort: 'asc' }] },
      }}
      rows={rows}
    />
  )
}

export default function FavoritesDrawer({
  onClose,
  opened,
  siteId,
}: {
  onClose: () => void
  opened: boolean
  siteId: string
}) {
  const analytics = useAnalytics()
  const dateTime = useDateTime()
  const queryClient = useQueryClient()
  const sites = useSites()
  const snackbar = useSnackbar()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()
  const user = useUser()
  const { isScopeSelectorEnabled, scopeId } = useScopeSelector()

  const userFavorites: TimeSeriesFavorite[] =
    user.options.timeMachineFavorites ?? []

  function loadFavorite(favorite: TimeSeriesFavorite, kind: FavoriteKind) {
    let { timeZoneOption, timeZone } = favorite

    // If timeZoneOption is related to a site, construct a timeZoneOption
    // from the site to ensure the site's timeZone is up to date.
    if (timeZoneOption?.siteId) {
      const tzSiteId = timeZoneOption.siteId
      const tzSite = sites.find((nextSite) => nextSite.id === tzSiteId)

      // Construct timeZoneOption from site.
      if (tzSite) {
        timeZoneOption = { timeZoneId: tzSite.timeZoneId, siteId: tzSite.id }
        timeZone = tzSite.timeZone
      }
    }

    // Getting current time (now) with timeZone in deriving range for "This month" or "Prev month"
    const now = dateTime.now(timeZone)
    const times = favorite.quickSelectTimeRange
      ? getDateTimeRange(now, favorite.quickSelectTimeRange)
      : [
          now.addMilliseconds(-favorite.timeDiffs[0]).format(),
          now.addMilliseconds(-favorite.timeDiffs[1]).format(),
        ]

    timeSeries.setTimeZoneOption(timeZoneOption)
    timeSeries.setTimeRange(favorite.quickSelectTimeRange)
    timeSeries.setState({
      ...timeSeries.state,
      kind,
      name: favorite.name,
      type: favorite.type,
      granularity: favorite.granularity,
      siteEquipmentIds: favorite.siteEquipmentIds,
      sitePointIds: favorite.sitePointIds,
      times,
    })

    analytics.track('Time Series Favorite Loaded', {
      favorite,
      group: `${TrackingTextByFavoriteKind[kind]}  Favorites`,
    })

    onClose()
  }

  function getRemoveFavorite(favorites: TimeSeriesFavorite[]) {
    return async (favorite: TimeSeriesFavorite, kind: FavoriteKind) => {
      analytics.track(
        `Time Series ${TrackingTextByFavoriteKind[kind]} Favorite deleted`,
        {
          favorite,
          favoriteName: favorite.name,
          favoriteEquipmentId: favorite.siteEquipmentIds[0],
        }
      )

      const updatedFavorites = favorites?.filter(
        (f) => f.name !== favorite.name
      )

      if (kind === 'site') {
        try {
          await api.put(`/sites/${siteId}/preferences/timeMachine`, {
            favorites: updatedFavorites,
          })

          queryClient.invalidateQueries(['timeSeriesSiteFavorites', siteId])
        } catch (err) {
          snackbar.show(t('plainText.errorSavingTimeSeries'))
        }
      } else if (kind === 'scope') {
        if (scopeId) {
          await updateTimeSeriesScopeFavorites(
            scopeId,
            updatedFavorites,
            queryClient
          )
        }
      } else {
        user.saveOptions('timeMachineFavorites', updatedFavorites)
      }
    }
  }

  return (
    <Drawer
      header={t('headers.favorites')}
      opened={opened}
      onClose={onClose}
      size="lg"
    >
      <DrawerBody>
        <Heading>{t('plainText.myFavs')}</Heading>
        {userFavorites.length ? (
          <FavoritesDataGrid
            dateTime={dateTime}
            kind="personal"
            onLoadFavorite={loadFavorite}
            onRemoveFavorite={getRemoveFavorite(userFavorites)}
            rows={userFavorites}
            t={t}
            timeSeries={timeSeries}
          />
        ) : (
          <NotFound>{t('plainText.noPersonalFavsFound')}</NotFound>
        )}

        {isScopeSelectorEnabled ? (
          scopeId ? (
            <ScopeFavoritesSection
              scopeId={scopeId}
              getRemoveFavorite={getRemoveFavorite}
              dateTime={dateTime}
              loadFavorite={loadFavorite}
              timeSeries={timeSeries}
              kind="scope"
              headingText="plainText.sharedFavorites"
              notFoundText="plainText.noSharedFavFound"
            />
          ) : null
        ) : (
          <SiteFavoritesSections
            siteId={siteId}
            getRemoveFavorite={getRemoveFavorite}
            dateTime={dateTime}
            loadFavorite={loadFavorite}
            timeSeries={timeSeries}
            kind="site"
            headingText="plainText.siteFavorites"
            notFoundText="plainText.noSiteFavFound"
          />
        )}
      </DrawerBody>
    </Drawer>
  )
}

interface FavoritesSectionsProps {
  favoritesData: UseQueryResult<
    {
      favorites?: TimeSeriesFavorite[]
    },
    unknown
  >
  removeFavorite: (favorite: TimeSeriesFavorite, kind: FavoriteKind) => void
  dateTime: ReturnType<typeof useDateTime>
  loadFavorite: (favorite: TimeSeriesFavorite, kind: FavoriteKind) => void
  timeSeries: TimeSeries
  headingText: string
  notFoundText: string
  kind: FavoriteKind
}

type SharedPropsForFavoritesSections = {
  getRemoveFavorite: (
    favorites: TimeSeriesFavorite[]
  ) => (favorite: TimeSeriesFavorite, kind: FavoriteKind) => Promise<void>
} & Omit<FavoritesSectionsProps, 'favoritesData' | 'removeFavorite'>

function FavoritesSections({
  favoritesData,
  removeFavorite,
  loadFavorite,
  dateTime,
  timeSeries,
  headingText,
  notFoundText,
  kind,
}: FavoritesSectionsProps) {
  const { t } = useTranslation()

  return (
    <>
      <Heading>{t(headingText)}</Heading>
      {favoritesData?.data?.favorites?.length ? (
        <FavoritesDataGrid
          dateTime={dateTime}
          kind={kind}
          onLoadFavorite={loadFavorite}
          onRemoveFavorite={removeFavorite}
          rows={favoritesData.data.favorites}
          t={t}
          timeSeries={timeSeries}
        />
      ) : (
        <NotFound>{t(notFoundText)}</NotFound>
      )}
    </>
  )
}

function SiteFavoritesSections({
  siteId,
  getRemoveFavorite,
  ...rest
}: {
  siteId: string
} & SharedPropsForFavoritesSections) {
  const siteFavoritesQuery = useGetTimeSeriesSiteFavorites(siteId)

  return (
    <FavoritesSections
      favoritesData={siteFavoritesQuery}
      removeFavorite={getRemoveFavorite(
        siteFavoritesQuery.data?.favorites ?? []
      )}
      {...rest}
    />
  )
}

function ScopeFavoritesSection({
  scopeId,
  getRemoveFavorite,
  ...rest
}: {
  scopeId: string
} & SharedPropsForFavoritesSections) {
  const scopeFavoritesQuery = useTimeSeriesScopeFavorites(scopeId)

  return (
    <FavoritesSections
      favoritesData={scopeFavoritesQuery}
      removeFavorite={getRemoveFavorite(
        scopeFavoritesQuery.data?.favorites ?? []
      )}
      {...rest}
    />
  )
}

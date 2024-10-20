import { render, screen } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { TimeSeriesContext } from '../../../components/TimeSeries/TimeSeriesContext'
import { TimeSeries, TimeSeriesFavorite, TimeSeriesState } from '../types'
import FavoritesDrawer from './FavoritesDrawer'

describe('FavoritesDrawer', () => {
  const SITE_ID = 'TEST_SITE_ID' as const

  function getWrapper({
    favorites = [],
    timeSeriesState = {},
    timeSeriesValue = {},
  }: {
    favorites: TimeSeriesFavorite[]
    timeSeriesState?: Partial<TimeSeriesState>
    timeSeriesValue?: Partial<Omit<TimeSeries, 'state'>>
  }) {
    return ({ children }) => (
      <BaseWrapper user={{ options: { timeMachineFavorites: favorites } }}>
        <TimeSeriesContext.Provider
          value={getTimeSeriesValue(timeSeriesState, timeSeriesValue)}
        >
          {children}
        </TimeSeriesContext.Provider>
      </BaseWrapper>
    )
  }

  function getTimeSeriesValue(
    timeSeriesState: Partial<TimeSeriesState> = {},
    timeSeriesValue: Partial<Omit<TimeSeries, 'state'>> = {}
  ): Partial<TimeSeries> {
    return {
      state: {
        granularity: 'PT15M',
        siteEquipmentIds: ['SiteA_EquipA', 'SiteB_EquipX'],
        siteId: SITE_ID,
        sitePointIds: ['A', 'B', 'C'],
        times: ['2022-04-20T11:00:00.000Z', '2022-04-23T11:00:00.000Z'],
        type: 'asset',
        ...timeSeriesState,
      },
      ...timeSeriesValue,
    }
  }

  function makeFavorite(
    favorite: Partial<TimeSeriesFavorite> = {}
  ): TimeSeriesFavorite {
    return {
      granularity: 'PT15M',
      name: 'My preset',
      siteEquipmentIds: ['SiteA_Equipment1'],
      sitePointIds: ['SiteA_Equipment1-PointA'],
      timeDiffs: [300, 0],
      type: 'asset',
      ...favorite,
    }
  }

  const oneHourInMilliseconds = 60 * 60 * 1000

  const timeSeriesState: Partial<TimeSeriesState> = {
    granularity: 'PT15M',
    siteEquipmentIds: ['twin1', 'twin3'],
    sitePointIds: ['A', 'B', 'C'],
    times: ['2022-04-20T11:00:00.000Z', '2022-04-23T11:00:00.000Z'],
    type: 'asset',
  }

  const favoriteWithSamePointsAndDateRange: Partial<TimeSeriesFavorite> = {
    granularity: 'PT30M',
    siteEquipmentIds: ['twin3', 'twin1'],
    sitePointIds: ['C', 'A', 'B'],
    timeDiffs: [72 * oneHourInMilliseconds, 0],
    timeZoneOption: { timeZoneId: 'AUS Eastern Standard Time' },
    type: 'stacked',
  }

  const favoriteWithSamePointsAndQuickRange: Partial<TimeSeriesFavorite> = {
    granularity: 'PT30M',
    quickSelectTimeRange: '48H',
    siteEquipmentIds: ['twin3', 'twin1'],
    sitePointIds: ['C', 'A', 'B'],
    timeDiffs: [3000, 20],
    timeZoneOption: { timeZoneId: 'AUS Eastern Standard Time' },
    type: 'stacked',
  }

  test('Favorite should show loaded when points and custom date range selection are the same as TimeSeries value', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [makeFavorite(favoriteWithSamePointsAndDateRange)],
        timeSeriesState,
        timeSeriesValue: {
          timeZoneOption: { timeZoneId: 'Eastern Standard Time' },
        },
      }),
    })

    expect(await screen.findByText('plainText.loaded')).toBeVisible()
  })

  test('Favorite should show loaded when points and quick range selection are the same as TimeSeries value', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [makeFavorite(favoriteWithSamePointsAndQuickRange)],
        timeSeriesState,
        timeSeriesValue: {
          quickRange: '48H',
          timeZoneOption: { timeZoneId: 'Eastern Standard Time' },
        },
      }),
    })

    expect(await screen.findByText('plainText.loaded')).toBeVisible()
  })

  test('Favorite should show load when points selection does not match selection in TimeSeries', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [
          makeFavorite({
            ...favoriteWithSamePointsAndDateRange,
            sitePointIds: ['C', 'A'],
          }),
        ],
        timeSeriesState,
      }),
    })

    expect(await screen.findByText('plainText.load')).toBeVisible()
  })

  test('Favorite should show load when time duration does not match selected quick range in TimeSeries', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [
          makeFavorite({
            ...favoriteWithSamePointsAndDateRange,
            quickSelectTimeRange: '48H',
          }),
        ],

        timeSeriesState,
        timeSeriesValue: { quickRange: 'thisMonth' },
      }),
    })

    expect(await screen.findByText('plainText.load')).toBeVisible()
  })

  test('Favorite should show load when time duration does not match date range selection in TimeSeries', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [
          makeFavorite({
            ...favoriteWithSamePointsAndDateRange,
            timeDiffs: [72 * oneHourInMilliseconds, 1],
          }),
        ],

        timeSeriesState,
        timeSeriesValue: { quickRange: 'thisMonth' },
      }),
    })

    expect(await screen.findByText('plainText.load')).toBeVisible()
  })

  test('Favorite should show load when siteEquipment does not match', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [
          makeFavorite({
            ...favoriteWithSamePointsAndDateRange,
            siteEquipmentIds: ['twin3', 'twin1', 'twin2'],
          }),
        ],

        timeSeriesState,
      }),
    })

    expect(await screen.findByText('plainText.load')).toBeVisible()
  })

  test('Favorites should be listed in alphabetical order', async () => {
    render(<FavoritesDrawer siteId={SITE_ID} onClose={jest.fn()} opened />, {
      wrapper: getWrapper({
        favorites: [
          makeFavorite({ name: 'Group Name' }),
          makeFavorite({ name: 'Alpha Example' }),
          makeFavorite({ name: 'comfort Example' }),
        ],
      }),
    })

    const listItems = screen
      .getAllByRole('cell')
      .filter((item) => item.getAttribute('data-field') === 'name')

    expect(listItems.length).toBe(3)
    expect(listItems[0].textContent).toBe('Alpha Example')
    expect(listItems[1].textContent).toBe('comfort Example')
    expect(listItems[2].textContent).toBe('Group Name')
  })
})

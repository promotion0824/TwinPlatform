/* eslint-disable import/no-extraneous-dependencies */
import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { NotificationSource } from '@willow/common/notifications/types'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { Snackbars } from '@willowinc/ui'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import useMultipleSearchParams from '../../../../common/src/hooks/useMultipleSearchParams'
import {
  makeNotification,
  makeNotificationsStats,
} from '../../mockServer/notificationItems'
import routes from '../../routes'
import Notifications from './Notifications'

// CommandLayoutHeader is not relevant to this test
jest.mock('../Command/CommandLayoutHeader', () => () => <div />)
jest.mock('../../../../common/src/hooks/useMultipleSearchParams')
const mockedUseMultipleSearchParams = jest.mocked(useMultipleSearchParams)

const server = setupServer(
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json({}))),
  rest.post('api/notifications/all', (req, res, ctx) => {
    const { filterSpecifications = [] } = req.body as any
    const filteredNotifications = originalNotifications.filter((notification) =>
      filterSpecifications.every((filter) => {
        if (filter.operator === 'contains') {
          return notification.title?.includes(filter.value)
        }
        return false
      })
    )

    return res(
      ctx.json({
        before: 0,
        after: 0,
        total: filteredNotifications.length,
        items: filteredNotifications,
      })
    )
  }),
  rest.post('api/notifications/states/stats', (_req, res, ctx) =>
    res(ctx.json(makeNotificationsStats(originalNotifications)))
  )
)

const insightIdOne = 'insight-1'
const insightIdTwo = 'insight-2'
const twinNameOne = 'Air Handling Unit AC-10-1'
const twinNameTwo = 'Air Handling Unit AC-10-2'
const titleOne = 'title-1'
const titleTwo = 'title-2'
const inputTestId = 'search-input'
const randomText = 'something-not-going-to-match'
const noMatchingResultsTestId = 'no-notifications'
const errorLoadingNotifications = 'Error Loading Notifications'
const refresh = 'Refresh'
const noNotificationsLastThirtyDays =
  'There are no notifications from the last 30 days.'
const noMatchingResults = 'No Matching Results'
const tryAnotherKeyword = 'Try Another Keyword'
const thatIsAllYourNotifications =
  "That's all your notifications from the last 30 days."

const [notificationOne, notificationTwo] = [
  makeNotification({
    id: 'notification-1',
    source: NotificationSource.Insight,
    propertyBagJson: JSON.stringify({
      entityId: insightIdOne,
      twinId: 'twin-1',
      twinName: twinNameOne,
      priority: 4,
    }),
    title: titleOne,
  }),
  makeNotification({
    id: 'notification-2',
    source: NotificationSource.Insight,
    propertyBagJson: JSON.stringify({
      entityId: insightIdTwo,
      twinId: 'twin-2',
      twinName: twinNameTwo,
      priority: 4,
    }),
    createdDateTime: new Date(Date.now() - 24 * 60 * 1000).toISOString(),
    title: titleTwo,
  }),
]
const originalNotifications = [notificationOne, notificationTwo]

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.restoreAllMocks()
  mockedUseMultipleSearchParams.mockClear()
})
afterAll(() => server.close())

describe('Notifications', () => {
  test('Expect to see no more notifications message when there is no notifications', async () => {
    mockedUseMultipleSearchParams.mockImplementation(() => [{}, jest.fn()])
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(ctx.json({ before: 0, after: 0, total: 0, items: [] }))
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json([]))
      )
    )

    render(<Notifications />, { wrapper: Wrapper })
    await waitFor(() => {
      expect(screen.queryByTestId(noMatchingResultsTestId)).toBeInTheDocument()
    })
    expect(
      screen.queryByText(noNotificationsLastThirtyDays)
    ).toBeInTheDocument()
  })

  test('Expect to see no matching results message when user search something but no notifications are matched', async () => {
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        search: randomText,
      },
      jest.fn(),
    ])
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(ctx.json({ before: 0, after: 0, total: 0, items: [] }))
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json([]))
      )
    )

    render(<Notifications />, { wrapper: Wrapper })
    await waitFor(() => {
      expect(screen.queryByTestId(noMatchingResultsTestId)).toBeInTheDocument()
    })

    expect(
      // There is a line break between the two texts
      screen.queryByText(
        new RegExp(`${noMatchingResults}\\s*${tryAnotherKeyword}`)
      )
    ).toBeInTheDocument()
  })

  test('Expect to see error message when fetching fail', async () => {
    mockedUseMultipleSearchParams.mockImplementation(() => [{}, jest.fn()])
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json([]))
      )
    )
    render(<Notifications />, { wrapper: Wrapper })

    await waitFor(() => {
      expect(screen.getByText(errorLoadingNotifications)).toBeInTheDocument()
    })
    const refreshButton = screen.getByText(refresh)
    expect(refreshButton).toBeInTheDocument()
    const notifications = [notificationOne, notificationTwo]
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(
          ctx.json({
            before: 0,
            after: 0,
            total: 2,
            items: notifications,
          })
        )
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json(notifications))
      )
    )
    // Click on "Refresh" button and refetch data which succeeds, and expect to see content
    act(() => {
      userEvent.click(refreshButton)
    })
    await waitFor(() => {
      expect(screen.getByText(titleOne)).toBeInTheDocument()
    })
    expect(screen.getByText(thatIsAllYourNotifications)).toBeInTheDocument()
  })

  test('Notifications should work as expected', async () => {
    const mockedSetSearchParams = jest.fn()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {},
      mockedSetSearchParams,
    ])
    const { rerender } = render(<Notifications />, { wrapper: Wrapper })

    await waitFor(() => {
      expect(screen.getByText(titleOne)).toBeInTheDocument()
      expect(screen.getByText(titleTwo)).toBeInTheDocument()
    })

    const searchInput = screen.queryByTestId(inputTestId)
    expect(searchInput).toBeInTheDocument()

    // Type in search input and expect to see search params updated
    act(() => {
      userEvent.type(searchInput!, titleTwo)
    })
    await waitFor(() => {
      expect(mockedSetSearchParams).toBeCalledWith({ search: titleTwo })
    })
    mockedUseMultipleSearchParams.mockReset()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        search: titleTwo,
      },
      mockedSetSearchParams,
    ])
    rerender(<Notifications />)
    // Wait for notification one disappear as it doesn't match the search
    await waitFor(() => {
      expect(screen.queryByText(titleOne)).not.toBeInTheDocument()
      expect(screen.queryByText(titleTwo)).toBeInTheDocument()
    })

    // Type in a random search phrase and expect to see no match
    act(() => {
      userEvent.type(screen.queryByTestId(inputTestId)!, randomText)
    })
    mockedUseMultipleSearchParams.mockReset()
    mockedUseMultipleSearchParams.mockImplementation(() => [
      {
        search: randomText,
      },
      mockedSetSearchParams,
    ])
    rerender(<Notifications />)

    await waitFor(() => {
      expect(screen.queryByTestId(noMatchingResultsTestId)).toBeInTheDocument()
    })
    expect(screen.queryByText(titleOne)).not.toBeInTheDocument()
    expect(screen.queryByText(titleTwo)).not.toBeInTheDocument()
  })
})

const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'plainText.errorLoadingNotifications': errorLoadingNotifications,
            'plainText.refresh': refresh,
            'plainText.noNotificationsLastThirtyDays':
              noNotificationsLastThirtyDays,
            'plainText.noMatchingResults': noMatchingResults,
            'plainText.tryAnotherKeyword': tryAnotherKeyword,
            'plainText.thatIsAllYourNotifications': thatIsAllYourNotifications,
          },
        },
      },
      lng: 'en',
      fallbackLng: ['en'],
    }}
    user={{
      customer: { features: { isDynamicsIntegrationEnabled: false } } as any,
      saveOptions: jest.fn(),
      options: {
        exportedTimeMachineCsvs: [],
      } as any,
    }}
    initialEntries={[routes.notifications]}
  >
    <Snackbars />
    {children}
  </BaseWrapper>
)

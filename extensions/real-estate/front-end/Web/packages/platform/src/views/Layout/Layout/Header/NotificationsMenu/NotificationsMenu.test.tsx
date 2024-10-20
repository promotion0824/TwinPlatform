/* eslint-disable import/no-extraneous-dependencies */
import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import {
  NotificationSource,
  NotificationStatus,
} from '@willow/common/notifications/types'
import { assertPathnameContains } from '@willow/common/utils/testUtils/LocationDisplay'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { Snackbars } from '@willowinc/ui'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import ReactRouter from 'react-router'
import {
  makeNotification,
  makeNotificationsStats,
} from '../../../../../mockServer/notificationItems'
import routes from '../../../../../routes'
import NotificationHeader from './NotificationsMenu'

const server = setupServer(
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json({})))
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.restoreAllMocks()
  jest.useRealTimers()
})
afterAll(() => server.close())

const insightIdOne = 'insight-1'
const insightIdTwo = 'insight-2'
const titleOne = 'title-1'
const titleTwo = 'title-2'
const bellIconTestId = 'bell-icon'
const markAllAsRead = 'Mark All as Read'
const undo = 'Undo'
const anErrorOccurred = 'An error occurred'
const errorLoadingNotifications = 'Error Loading Notifications'
const pleaseTryAgain = 'Please try again.'
const refresh = 'Refresh'
const viewAll = 'View All'
const oneNotificationMarkedAsRead = '1 notification marked as read.'

const [notificationOne, notificationTwo] = [
  makeNotification({
    id: 'notification-1',
    source: NotificationSource.Insight,
    propertyBagJson: JSON.stringify({
      entityId: insightIdOne,
      twinId: 'twin-1',
      twinName: 'Air Handling Unit AC-10-1',
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
      twinName: 'Air Handling Unit AC-10-2',
      priority: 4,
    }),
    createdDateTime: new Date(Date.now() - 24 * 60 * 1000).toISOString(),
    title: titleTwo,
  }),
]

describe('NotificationsMenu', () => {
  test('Expect to see error when fetching fail', async () => {
    const mockedOnOpen = jest.fn()
    const mockedOnChange = jest.fn()
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json([]))
      )
    )

    const { rerender } = render(
      <NotificationHeader
        onOpen={mockedOnOpen}
        onChange={mockedOnChange}
        isOpened={false}
      />,
      { wrapper: Wrapper }
    )
    // Expect to see bell icon
    const bellIcon = screen.queryByTestId(bellIconTestId)
    expect(bellIcon).toBeInTheDocument()

    // Click on bell icon and expect to see error message
    act(() => {
      userEvent.click(bellIcon!)
    })
    expect(mockedOnOpen).toBeCalled()
    rerender(
      <NotificationHeader
        onOpen={mockedOnOpen}
        onChange={mockedOnChange}
        isOpened
      />
    )
    await waitFor(() => {
      expect(screen.getByText(errorLoadingNotifications)).toBeInTheDocument()
    })

    const refreshButton = screen.getByText(refresh)
    const notifications = [notificationOne, notificationTwo]
    expect(refreshButton).toBeInTheDocument()
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
        res(ctx.json(makeNotificationsStats(notifications)))
      )
    )
    // Click on "Refresh" button and refetch data which succeeds, and expect to see content
    act(() => {
      userEvent.click(refreshButton)
    })
    await waitFor(() => {
      expect(screen.getByText(titleOne)).toBeInTheDocument()
    })

    const viewAllButton = screen.getByText(viewAll)
    expect(viewAllButton).toBeInTheDocument()

    // Click on "View All" button and expect
    // user has been navigated to notifications page and Menu is closed by
    // calling onChange
    act(() => {
      userEvent.click(viewAllButton)
    })
    expect(mockedOnChange).toBeCalled()
    assertPathnameContains(routes.notifications)
  })

  test('NotificationMenu should work as expected', async () => {
    jest.useFakeTimers()
    const originalNotifications = [notificationOne, notificationTwo]

    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(
          ctx.json({
            before: 0,
            after: 0,
            total: originalNotifications.length,
            items: originalNotifications,
          })
        )
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json(makeNotificationsStats(originalNotifications)))
      )
    )

    const mockedPush = jest.fn()
    jest.spyOn(ReactRouter, 'useHistory').mockReturnValue({
      push: mockedPush,
    } as any)

    const { rerender } = render(
      <NotificationHeader
        onOpen={jest.fn()}
        onChange={jest.fn()}
        isOpened={false}
      />,
      { wrapper: Wrapper }
    )
    const bellIcon = screen.queryByTestId(bellIconTestId)
    expect(bellIcon).toBeInTheDocument()

    // Click on bell icon and expect to see content
    act(() => {
      userEvent.click(bellIcon!)
    })
    rerender(
      <NotificationHeader onOpen={jest.fn()} onChange={jest.fn()} isOpened />
    )
    await waitFor(() => {
      expect(screen.getByRole('menu')).toBeInTheDocument()
    })
    // Expect to see the first notification
    await waitFor(() => {
      expect(screen.getByText(titleOne)).toBeInTheDocument()
    })

    // Expect to navigate to insight page when click on a notification
    act(() => {
      userEvent.click(screen.getByText(titleOne))
    })
    expect(mockedPush).toBeCalledWith(
      routes.insights_insight__insightId(insightIdOne)
    )

    // Expect to see radios are checked for all notifications
    // as they are all in "new" state
    const allRadioInputs = screen.queryAllByRole('radio')
    for (const radio of allRadioInputs) {
      expectRadioToBeChecked(radio, true)
    }

    // Click on first radio to mark it as read
    act(() => {
      userEvent.click(allRadioInputs[0])
    })

    const updatedNotifications = originalNotifications.map(
      (notification, index) =>
        index === 0
          ? {
              ...notification,
              state: NotificationStatus.Open,
            }
          : notification
    )
    server.use(
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(
          ctx.json({
            before: 0,
            after: 0,
            total: updatedNotifications.length,
            items: updatedNotifications,
          })
        )
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json(makeNotificationsStats(updatedNotifications)))
      ),
      rest.put('api/notifications/state', (_req, res, ctx) =>
        res(ctx.status(204))
      )
    )

    // Now first notification should be marked as read (radio should be unchecked)
    // 2nd notification should be still unread (radio should be checked)
    await waitFor(() => {
      assertOnlyFirstNotificationHasRadioUnchecked()
    })

    const markAllAsReadButton = screen.getByText(markAllAsRead)
    expect(markAllAsReadButton).toBeInTheDocument()

    // Expect to immediately see all notification's radios are unchecked
    // when click on "Mark All as Read" button
    act(() => {
      userEvent.click(markAllAsReadButton)
    })
    for (const radio of screen.queryAllByRole('radio')) {
      expectRadioToBeChecked(radio, false)
    }
    // Expect to see "Undo" button showing up in a snackbar.
    await waitFor(() => {
      expect(screen.getByText(undo)).toBeInTheDocument()
    })
    // Expect to see all notifications' statuses are reverted back to previous versions
    // when user clicks on "Undo" button
    act(() => {
      userEvent.click(screen.getByText(undo))
    })
    assertOnlyFirstNotificationHasRadioUnchecked()

    // Click on Mark All as Read button again and this time PUT request fails
    act(() => {
      userEvent.click(markAllAsReadButton)
    })
    for (const radio of screen.queryAllByRole('radio')) {
      expectRadioToBeChecked(radio, false)
    }
    server.use(
      rest.put('api/notifications/state', (_req, res, ctx) =>
        res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
      )
    )

    // Simulate the passage of 5 seconds
    jest.advanceTimersByTime(5000)
    await waitFor(
      () => {
        expect(screen.getByText(anErrorOccurred)).toBeInTheDocument()
      },
      // Snackbar will disappear after 4 seconds, so we slightly
      // wait longer for error message to show up
      { timeout: 5000 }
    )
    // All states should be reverted back since the PUT request failed
    assertOnlyFirstNotificationHasRadioUnchecked()

    // Click on Mark All as Read button again and PUT request succeeds
    act(() => {
      userEvent.click(markAllAsReadButton)
    })
    for (const radio of screen.queryAllByRole('radio')) {
      expectRadioToBeChecked(radio, false)
    }

    const noMoreUnreadNotifications = originalNotifications.map(
      (notification) => ({
        ...notification,
        state: NotificationStatus.Open,
      })
    )
    server.use(
      rest.put('api/notifications/state', (_req, res, ctx) =>
        // put request succeeds
        res(ctx.status(204))
      ),
      rest.post('api/notifications/all', (_req, res, ctx) =>
        res(
          ctx.json({
            before: 0,
            after: 0,
            total: 2,
            items: noMoreUnreadNotifications,
          })
        )
      ),
      rest.post('api/notifications/states/stats', (_req, res, ctx) =>
        res(ctx.json(makeNotificationsStats(noMoreUnreadNotifications)))
      )
    )
    jest.advanceTimersByTime(5000)
    // All states should be unchecked since the PUT request succeeded
    for (const radio of screen.queryAllByRole('radio')) {
      expectRadioToBeChecked(radio, false)
    }
    // Expect to see a snackbar showing up with correct message
    expect(screen.getByText(oneNotificationMarkedAsRead)).toBeInTheDocument()

    jest.useRealTimers()
    // Now "Mark All as Read" button should be gone
    await waitFor(() => {
      expect(screen.queryByText(markAllAsRead)).not.toBeInTheDocument()
    })
  })
})

const assertOnlyFirstNotificationHasRadioUnchecked = () => {
  const allRadioInputs = screen.queryAllByRole('radio')
  for (const [index, radio] of allRadioInputs.entries()) {
    expectRadioToBeChecked(radio, index !== 0)
  }
}

const expectRadioToBeChecked = (
  inputInRadio: HTMLElement,
  isChecked: boolean
) => {
  if (isChecked) {
    expect(inputInRadio).toHaveStyle({
      color: 'inherit',
    })
  } else {
    expect(inputInRadio).toHaveStyle({
      color: 'transparent',
    })
  }
}
const Wrapper = ({ children }) => (
  <BaseWrapper
    i18nOptions={{
      resources: {
        en: {
          translation: {
            'plainText.markAllAsRead': markAllAsRead,
            'plainText.undo': undo,
            'plainText.anErrorOccurred': anErrorOccurred,
            'plainText.errorLoadingNotifications': errorLoadingNotifications,
            'plainText.pleaseTryAgain': pleaseTryAgain,
            'plainText.refresh': refresh,
            'plainText.viewAll': viewAll,
            'interpolation.countOfNotificationsMarkedAsRead':
              oneNotificationMarkedAsRead,
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
  >
    <Snackbars />
    {children}
  </BaseWrapper>
)

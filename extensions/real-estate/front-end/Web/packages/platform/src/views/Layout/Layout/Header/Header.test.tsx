/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { lastDateTimeOpenNotificationBellKey } from '@willow/common/notifications'
import {
  NotificationSource,
  NotificationStatus,
} from '@willow/common/notifications/types'
import '@willow/common/utils/testUtils/matchMediaMock'
import { UserContext } from '@willow/ui/providers/UserProvider/UserContext'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { noop } from 'lodash'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import { useState } from 'react'
import { MemoryRouter } from 'react-router'
import { makeNotification } from '../../../../mockServer/notificationItems'
import SiteProvider from '../../../../providers/sites/SiteProvider'
import SitesProvider from '../../../../providers/sites/SitesStubProvider'
import LayoutProvider from '../Layout'

const notifications = Array.from({ length: 10 }, (_, i) =>
  makeNotification({
    id: `id-${i}`,
    source: NotificationSource.Insight,
    propertyBagJson: '',
    title: `title-${i}`,
  })
)
const server = setupServer(
  rest.get('/api/sites/:siteId/floors', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/sites/:siteId/models', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/contactus/categories', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/me/preferences', (_req, res, ctx) => res(ctx.json({}))),
  rest.post('/api/notifications/all', (_req, res, ctx) =>
    res(
      ctx.json({
        after: 0,
        before: 0,
        total: 10,
        items: notifications,
      })
    )
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  jest.restoreAllMocks()
  jest.useRealTimers()
})
afterAll(() => server.close())

describe('Header component', () => {
  test('Displays the bell icon with the new notifications count since it was last opened', async () => {
    server.use(
      rest.post('/api/notifications/states/stats', (req, res, ctx) => {
        const { body } = req
        const lastDateTimeOpenNotificationBell = (body as any).find(
          ({ field }) => field === 'notification.createdDateTime'
        )?.value

        return res(
          ctx.json([
            {
              state: NotificationStatus.New,
              // Mimic a situation where there are total of 10 notifications
              // since last time the bell icon was opened
              count:
                new Date().valueOf() -
                  new Date(lastDateTimeOpenNotificationBell).valueOf() >
                24 * 60 * 60 * 1000 // 1 day in milliseconds
                  ? notifications.length
                  : 0,
            },
          ])
        )
      }),
      rest.get('/api/me/preferences', (_req, res, ctx) =>
        res(
          ctx.json({
            profile: {
              [lastDateTimeOpenNotificationBellKey]: new Date(
                // Last time the bell icon was opened more than 1 day ago
                Date.now() - (24 * 60 * 60 * 1000 + 1)
              ).toISOString(),
            },
          })
        )
      )
    )

    render(<Wrapper customer={{}} />)
    await waitFor(() => {
      expect(
        screen.queryByText(notifications.length.toString())
      ).toBeInTheDocument()
    })

    // Expect to see bell icon
    const bellIcon = screen.queryByTestId(bellIconTestId)
    expect(bellIcon).toBeInTheDocument()

    // Click on bell icon and expect to see the number of notifications disappear
    act(() => {
      userEvent.click(bellIcon!)
    })

    // Update the server response to reflect the bell icon being opened
    server.use(
      rest.get('/api/me/preferences', (_req, res, ctx) =>
        res(
          ctx.json({
            profile: {
              [lastDateTimeOpenNotificationBellKey]: new Date().toISOString(),
            },
          })
        )
      )
    )

    await waitFor(() => {
      expect(screen.queryByText(notifications.length.toString())).toBeNull()
    })
  })

  test('display a hamburger menu button', () => {
    render(<Wrapper customer={{}} />)

    expect(screen.queryAllByTestId('menu-title')[0]).toBeInTheDocument()
  })

  test('display mainmenu when the hamburger menu button clicked', async () => {
    render(<Wrapper customer={{}} />)

    act(() => {
      fireEvent.click(screen.queryAllByTestId('menu-title')[0])
    })

    await waitFor(() =>
      expect(screen.getByText('headers.home')).toBeInTheDocument()
    )
  })

  test('display a Willow logo link', () => {
    const { container } = render(<Wrapper customer={{}} />)

    const homeButtonEl = container.querySelector(
      'a[data-segment="Willow Home Button Clicked"]'
    )
    expect(homeButtonEl).toBeInTheDocument()
  })

  const name = 'Investa'
  test("display a customer's name if the name exist in user info and the user is a Willow user", () => {
    render(<Wrapper customer={{ name }} userEmail="user@willowinc.com" />)
    expect(screen.queryAllByTestId('customer-name')[0]).toBeInTheDocument()
  })

  test("do not display a customer's name if the user isn't a Willow user", () => {
    render(<Wrapper customer={{ name }} userEmail="user@customer.com" />)
    expect(screen.queryAllByTestId('customer-name')).toHaveLength(0)
  })

  test("do not display a customer's name if the name does not exist in user info", () => {
    render(<Wrapper customer={{}} />)
    expect(screen.queryAllByTestId('customer-name')).toHaveLength(0)
  })

  test('display an user profile button', () => {
    render(<Wrapper customer={{}} />)
    expect(screen.queryAllByTestId('user-menu')[0]).toBeInTheDocument()
  })
})

function UserProvider({ customer = {}, children, email }) {
  const [user, setUser] = useState({})
  const value = {
    ...user,
    customer,
    email,
    saveOptions: jest.fn(),
    portfolios: [],
    saveLanguage: (lang) => {
      setUser((prevUser) => ({
        ...prevUser,
        language: lang,
      }))
    },
    saveLocalOptions: noop,
    options: {
      siteId: '1',
    },
    hasPermissions: jest.fn(),
  }
  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

const sites = [{ id: '1', features: {} }]
const bellIconTestId = 'bell-icon'

const Wrapper = ({
  customer,
  userEmail,
  children,
}: {
  customer: { name?: string }
  userEmail?: string
  children?: React.ReactNode
}) => (
  <BaseWrapper hasFeatureToggle={(feature) => feature !== 'globalSidebar'}>
    <UserProvider customer={customer} email={userEmail}>
      <MemoryRouter>
        <SitesProvider sites={sites}>
          <SiteProvider>
            {/* Header is rendered inside LayoutProvider */}
            <LayoutProvider>{children}</LayoutProvider>
          </SiteProvider>
        </SitesProvider>
      </MemoryRouter>
    </UserProvider>
  </BaseWrapper>
)

import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import Layout from '../Layout'

describe('Layout', () => {
  test('expect to see hamburger menu button even when fetching sites fail', async () => {
    setupServerWithReject()
    render(<Layout />, {
      wrapper: getWrapper({}),
    })

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    expect(screen.queryByText(errorMessage)).toBeInTheDocument()
    expect(screen.queryByTestId(hamburgerMenuTestId)).toBeInTheDocument()
  })

  test('expect to see spinner when fetching sites', async () => {
    setupServerWithDelay()
    render(<Layout />, {
      wrapper: getWrapper({}),
    })

    expect(screen.queryByRole('img', { name: 'loading' })).toBeInTheDocument()
  })
  test('expect to see spinner when feature flags are not loaded yet', async () => {
    render(<Layout />, {
      wrapper: getWrapper({ featureFlagsLoaded: false }),
    })

    expect(screen.queryByRole('img', { name: 'loading' })).toBeInTheDocument()
    sites.forEach((site) => {
      expect(screen.queryByText(site.name)).not.toBeInTheDocument()
    })
  })

  test('failure of loading feature flags should not block /me/sites call', async () => {
    render(
      <Layout>
        {/* instead of  asserting that a particular request was made which is discouraged,
            in this case /me/sites, we render a placeholder div for each site and make assertions
            on that
            reference: https://mswjs.io/docs/best-practices/avoid-request-assertions/
        */}
        {sites.map((site) => (
          <div key={site.id}>{site.name}</div>
        ))}
      </Layout>,
      {
        wrapper: getWrapper({
          featureFlagsLoaded: false,
          errorOnLoadFeatureFlags: true,
        }),
      }
    )

    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })
    // all sites should be rendered
    sites.forEach((site) => {
      expect(screen.queryByText(site.name)).toBeInTheDocument()
    })
  })

  test('expect to see hamburger menu button on Layout', async () => {
    render(<Layout />, {
      wrapper: getWrapper({}),
    })
    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    const hamburgerMenuButton = screen.queryByTestId(hamburgerMenuTestId)
    expect(hamburgerMenuButton).toBeInTheDocument()
    await act(async () => {
      userEvent.click(hamburgerMenuButton!)
    })

    // expect to see dashboard menu button and admin menu button
    // when client is set up with sites
    await waitFor(() => {
      expect(screen.queryByText(dashboardMenuButtonText)).not.toBeNull()
      expect(screen.queryByText(adminMenuButtonText)).not.toBeNull()
    })
  })

  test('expect to still see hamburger menu button on Layout when client is not set up with sites', async () => {
    setupServerWithNoSites()
    render(<Layout />, {
      wrapper: getWrapper({}),
    })
    await waitFor(() => {
      expect(screen.queryByRole('img', { name: 'loading' })).toBeNull()
    })

    const hamburgerMenuButton = screen.queryByTestId(hamburgerMenuTestId)
    expect(hamburgerMenuButton).toBeInTheDocument()
    await act(async () => {
      userEvent.click(hamburgerMenuButton!)
    })

    // new client with no sites set up will still be able to
    // click on hamburger menu and only see admin menu button
    await waitFor(() => {
      expect(screen.queryByText(adminMenuButtonText)).not.toBeNull()
    })
    expect(screen.queryByText(dashboardMenuButtonText)).toBeNull()
    expect(screen.queryByText(noSitesFoundMessage)).toBeInTheDocument()
  })
})

const handler = [
  rest.get('/api/me/sites', (_req, res, ctx) => res(ctx.json(sites))),
  rest.get('/api/sites/:siteId/floors', (_req, res, ctx) => res(ctx.json([]))),
  rest.get('/api/contactus/categories', (_req, res, ctx) => res(ctx.json([]))),
]
const setupServerWithDelay = () =>
  server.use(
    rest.get('/api/me/sites', (req, res, ctx) =>
      res(ctx.delay(), ctx.json(sites))
    )
  )
const setupServerWithReject = () =>
  server.use(
    rest.get('/api/me/sites', (req, res, ctx) =>
      res(ctx.status(400), ctx.json({ message: 'FETCH ERROR' }))
    )
  )
const setupServerWithNoSites = () =>
  server.use(rest.get('/api/me/sites', (_req, res, ctx) => res(ctx.json([]))))

const server = setupServer(...handler)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

const hamburgerMenuTestId = 'menu-title'
const dashboardMenuButtonText = 'headers.dashboards'
const adminMenuButtonText = 'headers.admin'
const errorMessage = 'plainText.errorOccurred'
const noSitesFoundMessage = 'plainText.noSitesFound'
const getSite = (id, name) => ({
  id,
  name,
})
const sites = [
  getSite('site-1', 'Site 1'),
  getSite('site-2', 'Site 2'),
  getSite('site-3', 'Site 3'),
]
const getWrapper =
  ({
    featureFlagsLoaded = true,
    errorOnLoadFeatureFlags = false,
  }: {
    featureFlagsLoaded?: boolean
    errorOnLoadFeatureFlags?: boolean
  }) =>
  ({ children }) =>
    (
      <BaseWrapper
        user={
          {
            portfolios: [],
            showAdminMenu: true,
          } as any
        }
        featureFlagsLoaded={featureFlagsLoaded}
        errorOnLoad={errorOnLoadFeatureFlags}
        hasFeatureToggle={(feature) => feature !== 'globalSidebar'}
      >
        {children}
      </BaseWrapper>
    )

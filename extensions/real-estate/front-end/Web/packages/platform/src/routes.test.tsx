import { render } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'

import SiteProvider from './providers/sites/SiteProvider'
import SitesProvider from './providers/sites/SitesStubProvider'
import routes, { getSitePage } from './routes'
import LayoutProvider from './views/Layout/Layout/Layout'
import SiteContent from './views/SiteContent'

describe('getSitePage', () => {
  test.each([
    ['/sites/952b3038-25c2-44e2-8204-666995d047d1/insights', 'insights'],
    [
      '/sites/a6b78f54-9875-47bc-9612-aa991cc464f3/tickets/schedules',
      'tickets',
    ],
    [
      '/marketplace/sites/e719ac18-192b-4174-91db-b3a624f1f1a4/my-apps',
      'my-apps',
    ],
    ['/sites/f1914666-4050-4ff7-afd7-013bae2eee97', ''],
    [
      '/admin/portfolios/152b987f-0da2-4e77-9744-0e5c52f6ff3d/sites/a6b78f54-9875-47bc-9612-aa991cc464f3/floors',
      'floors',
    ],
  ])('when pathname %s is a site page, return %s', (pathname, branch) => {
    expect(getSitePage(pathname)).toBe(branch)
  })

  test.each([
    ['/admin'],
    ['/admin/portfolios/152b987f-0da2-4e77-9744-0e5c52f6ff3d/connectivity'],
  ])('when pathname %s is not a site page, return nothing', (pathname) => {
    expect(getSitePage(pathname)).not.toBeDefined()
  })
})

// Mocks for the route tests below.
// Must be kept outside of the describe block in order for them to take effect.
jest.mock('./views/Portfolio/Portfolio', () =>
  jest.fn(({ children }) => children)
)

jest.mock('./views/Command/Dashboard/Dashboard', () =>
  jest.fn(() => (
    <>
      <div>Home</div>
      <div>Dashboards</div>
    </>
  ))
)

jest.mock('./views/Portfolio/twins/results/page/ui/SearchResults', () =>
  jest.fn(() => <div>Search & Explore</div>)
)

jest.mock('./views/Portfolio/Reports/Reports', () =>
  jest.fn(() => <div>Reports</div>)
)

jest.mock('./components/Insights/CardViewInsights/CardViewInsights', () =>
  jest.fn(() => <div>Insights</div>)
)

jest.mock('./views/Command/Tickets/Tickets', () =>
  jest.fn(() => <div>Tickets</div>)
)

jest.mock('./views/Command/Inspections/Inspections', () =>
  jest.fn(() => <div>Inspections</div>)
)

jest.mock('./views/Marketplace/Marketplace', () =>
  jest.fn(() => <div>Marketplace</div>)
)

jest.mock('./views/TimeSeries/TimeSeries', () =>
  jest.fn(() => <div>Time Series</div>)
)

jest.mock('./views/Portfolio/MapView/MapView', () =>
  jest.fn(() => <div>Map Viewer</div>)
)

jest.mock('./views/Admin/Admin', () => jest.fn(() => <div>Admin</div>))

// These tests are done with a site already being selected (with All Sites being disabled).
describe('major routes', () => {
  afterAll(() => jest.restoreAllMocks())

  const testSiteId = '952b3038-25c2-44e2-8204-666995d047d1'

  const majorRoutes = [
    {
      name: 'Home',
      route: routes.sites__siteId(testSiteId),
    },
    {
      name: 'Dashboards',
      route: routes.dashboards_sites__siteId(testSiteId),
    },
    {
      name: 'Search & Explore',
      route: routes.portfolio_twins_results,
    },
    {
      name: 'Reports',
      route: routes.sites__siteId_reports(testSiteId),
    },
    {
      name: 'Insights',
      route: routes.sites__siteId_insights(testSiteId),
    },
    {
      name: 'Tickets',
      route: routes.sites__siteId_tickets(testSiteId),
    },
    {
      name: 'Inspections',
      route: routes.sites__siteId_inspections(testSiteId),
    },
    {
      name: 'Marketplace',
      route: routes.connectors_sites__siteId(testSiteId),
    },
    {
      name: 'Time Series',
      route: routes.timeSeries,
    },
    {
      name: 'Map Viewer',
      route: routes.map_viewer,
    },
    {
      name: 'Admin',
      route: routes.admin,
    },
  ]

  const mockedPortfolios = [
    {
      id: '152b987f-0da2-4e77-9744-0e5c52f6ff3d',
      name: 'Portfolio 1',
    },
  ]

  const mockedSites = [
    {
      id: testSiteId,
      features: {
        isOccupancyEnabled: true,
      },
    },
  ]

  const mockedUser = { portfolios: mockedPortfolios, showAdminMenu: true }

  test.each(majorRoutes)(
    'should return the expected route for $name',
    async (routeDetails) => {
      const page = render(
        <Wrapper initialEntries={[routeDetails.route]} user={mockedUser}>
          <SitesProvider sites={mockedSites}>
            <SiteProvider>
              <LayoutProvider>
                <SiteContent />
              </LayoutProvider>
            </SiteProvider>
          </SitesProvider>
        </Wrapper>
      )

      expect(page.getByText(routeDetails.name)).toBeInTheDocument()
    }
  )
})

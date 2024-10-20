/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { render, screen, waitFor } from '@testing-library/react'
import { useUser } from '@willow/ui/providers/UserProvider/UserContext'
import Wrapper, { wrapperIsReady } from '@willow/ui/utils/testUtils/Wrapper'
import SiteStubProvider from '../../../../providers/sites/SiteStubProvider'
import routes from '../../../../routes'
import MainMenu from './MainMenu'

jest.mock('@willow/ui/providers/UserProvider/UserContext')
const mockUseUser = jest.mocked(useUser)

const getFeatureFlags = (flags: Record<string, boolean>) => ({
  hasFeatureToggle: (featureFlag: string) => flags[featureFlag],
})

describe('MainMenu component', () => {
  const defaultSites = [...Array(5)].map((_, i) => ({
    id: String(i + 1),
  }))
  const defaultSite = {
    id: '1',
  }
  const defaultUser = {
    portfolios: [],
    localOptions: {},
    options: {},
    showAdminMenu: true,
    showPortfolioTab: true,
    showRulingEngineMenu: true,
    hasPermissions: jest.fn().mockReturnValue(true),
  }

  const defaultLayout: { menuItems: any } = { menuItems: [] }

  const BaseWrapper = ({ children }) => (
    <Wrapper>
      <SiteStubProvider site={defaultSite}>{children}</SiteStubProvider>
    </Wrapper>
  )

  const renderMainMenu = async ({
    onClose = jest.fn(),
    sites = defaultSites,
    user = defaultUser,
    layout = defaultLayout,
    config = getFeatureFlags({}),
    featureFlags = getFeatureFlags({}),
  }) => {
    const renderResult = render(
      <MainMenu
        isOpen
        onClose={onClose}
        sites={sites}
        user={user}
        layout={layout}
        config={config}
        featureFlags={featureFlags}
      />,
      { wrapper: BaseWrapper }
    )
    await waitFor(() => expect(wrapperIsReady(screen)).toBeTrue())
    return renderResult
  }

  describe('Layout menu items display', () => {
    test('display menu buttons when there are menuItems from layout', async () => {
      const menuItems = [
        {
          id: 'item0',
          header: 'header0',
          subHeader: 'subHeader0',
          disabled: true,
          to: '/',
        },
      ]
      await renderMainMenu({ layout: { menuItems } })

      menuItems.forEach(({ subHeader }) => {
        expect(screen.getByText(subHeader)).toBeInTheDocument()
      })
    })

    describe('Site menu items display', () => {
      test('display default menu buttons when site selected', async () => {
        await renderMainMenu({})

        const menuItemNames = ['headers.connectors', 'headers.timeSeries']
        menuItemNames.forEach((name) => {
          expect(screen.getByText(name)).toBeInTheDocument()
        })
      })
    })

    describe('combinedView test', () => {
      test('Display required buttons', async () => {
        await renderMainMenu({
          featureFlags: getFeatureFlags({}),
        })

        const menuItemNames = [
          'headers.home',
          'headers.dashboards',
          'headers.reports',
        ]
        menuItemNames.forEach((name) => {
          expect(screen.getByText(name)).toBeInTheDocument()
        })
      })
      test('should go to dashboard if no selected site id', async () => {
        const expectedPortfolioDashboardPath = routes.dashboards

        await renderMainMenu({
          featureFlags: getFeatureFlags({}),
        })

        // expect Dashboard button to direct user to portfolio performance view
        // when last selected site id is not defined.
        expect(
          (await screen.findByText('headers.dashboards')).closest('a')?.href
          // assert with toEndWith to ignore domain in test
        ).toEndWith(expectedPortfolioDashboardPath)
      })

      test('should go to site dashboard if there is a selected site id ', async () => {
        const selectedSiteId = 'site-123'
        const expectedDashboardPath =
          routes.dashboards_sites__siteId(selectedSiteId)

        const user = {
          ...defaultUser,
          localOptions: {
            lastSelectedSiteId: selectedSiteId,
            scopeSelectorLocation: {
              twin: { siteId: selectedSiteId },
            },
          },
        }

        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          onClose: jest.fn(),
          sites: defaultSites,
          user,
          layout: defaultLayout,
          config: getFeatureFlags({}),
          featureFlags: getFeatureFlags({}),
        })

        // expect Dashboard button to direct user to site performance view
        // when last selected site id is defined
        expect(
          (await screen.findByText('headers.dashboards')).closest('a')?.href
        ).toEndWith(expectedDashboardPath)
      })
    })

    const anchorHasId = (text: string, id: string) =>
      screen.queryByText(text)!.closest('a')!.getAttribute('href')!.includes(id)

    describe('Insight Menu Button', () => {
      const targetText = 'plainText.viewAndSetInsights'
      test('expect insight menu button to be visible', async () => {
        await renderMainMenu({ featureFlags: getFeatureFlags({}) })
        expect(screen.queryByText(targetText)).toBeInTheDocument()
      })
    })

    describe('combined view tickets', () => {
      const targetText = 'plainText.reviewAndManageTickets'

      test('Always show tickets menu button', async () => {
        await renderMainMenu({})

        expect(screen.queryByText(targetText)).toBeInTheDocument()
      })

      test('Ticket Link should have Fav Id if All Sites is selected (If Fav site present)', async () => {
        const favoriteSiteId = '2'
        const lastSelectedSiteId = null
        const user = {
          ...defaultUser,
          options: { favoriteSiteId },
          localOptions: { lastSelectedSiteId },
        }
        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          user,
        })
        expect(
          screen
            .queryByText('headers.tickets')!
            .closest('a')!
            .getAttribute('href')!
            .includes(favoriteSiteId)
        ).toBeTrue()
      })

      test('Ticket link should contain path of "/tickets" (pathname for All Sites Tickets) if All Sites is selected (If Fav site is not present)', async () => {
        const lastSelectedSiteId = null
        const user = {
          ...defaultUser,
          localOptions: { lastSelectedSiteId },
        }
        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          user,
        })
        expectClosestLinkToBe({ linkText: 'headers.tickets', path: '/tickets' })
      })

      test('Ticket Link should contain lastSelectedSiteId if any other site than All Sites selected', async () => {
        const lastSelectedSiteId = '2'
        const user = {
          ...defaultUser,
          localOptions: { lastSelectedSiteId },
        }
        mockUseUser.mockImplementation(() => user)
        await renderMainMenu({
          user,
        })
        expect(
          screen
            .queryByText('headers.tickets')!
            .closest('a')!
            .getAttribute('href')!
            .includes(lastSelectedSiteId)
        ).toBeTrue()
      })
    })

    describe('site id in menu button navigation', () => {
      const lastSelectedSiteId = '2'
      const favoriteSiteId = '3'
      const insightsText = 'headers.insights'
      const inspectionsText = 'headers.inspections'
      test('Insights/Inspections menu should have a site link when a site is selected', async () => {
        const user = { ...defaultUser, localOptions: { lastSelectedSiteId } }
        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          user,
        })

        expect(anchorHasId(insightsText, lastSelectedSiteId)).toBeTrue()
        expect(anchorHasId(inspectionsText, lastSelectedSiteId)).toBeTrue()
      })

      test('Insights/Inspections menu should have favorite site link when favorite site is selected', async () => {
        const user = { ...defaultUser, options: { favoriteSiteId } }
        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          user,
        })

        expect(anchorHasId(insightsText, favoriteSiteId)).toBeTrue()
        expect(anchorHasId(inspectionsText, favoriteSiteId)).toBeTrue()
      })

      test('Insights/Inspections link should contain path of "/insights" or "/inspections" if All Sites is selected (If Fav site is not present)', async () => {
        const user = { ...defaultUser, options: {} }
        mockUseUser.mockImplementation(() => user)

        await renderMainMenu({
          user,
        })
        expectClosestLinkToBe({
          linkText: 'headers.insights',
          path: '/insights',
        })
        expectClosestLinkToBe({
          linkText: 'headers.inspections',
          path: '/inspections',
        })
      })
    })

    describe('wp flags', () => {
      test('Display rules when wp-rules-enabled is true and showRulingEngineMenu is true', async () => {
        await renderMainMenu({
          config: getFeatureFlags({ 'wp-rules-enabled': true }),
        })

        expect(screen.getByText('headers.rules')).toBeInTheDocument()
      })
    })

    describe('Connector menu item', () => {
      it('should have Connector menu item with permission true', async () => {
        await renderMainMenu({})
        expect(screen.getByText('headers.connectors')).toBeInTheDocument()
      })

      it('should not show Connectors menu item if no canViewConnectors permission', async () => {
        await renderMainMenu({
          user: {
            ...defaultUser,
            hasPermissions: jest.fn().mockReturnValue(false),
          },
        })

        expect(screen.queryByText('headers.connectors')).not.toBeInTheDocument()
      })

      it('should show Connectors menu item if permission is undefined', async () => {
        await renderMainMenu({
          user: {
            ...defaultUser,
            hasPermissions: jest.fn().mockReturnValue(undefined),
          },
        })

        expect(screen.getByText('headers.connectors')).toBeInTheDocument()
      })
    })
  })
})

const expectClosestLinkToBe = ({
  linkText,
  path,
}: {
  linkText: string
  path: string
}) =>
  expect(
    screen.queryByText(linkText)!.closest('a')!.getAttribute('href')!
  ).toBe(path)

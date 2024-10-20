import '@testing-library/jest-dom'
import { render } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { buildingModelId } from '@willow/common/twins/view/modelsOfInterest'
import { Button } from '@willow/ui'
import AnalyticsProvider from '@willow/ui/providers/AnalyticsProvider/AnalyticsStubProvider'
import { UserContext } from '@willow/ui/providers/UserProvider/UserContext'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { createMemoryHistory } from 'history'
import { useState } from 'react'
import { act } from 'react-dom/test-utils'
import { Router, useHistory } from 'react-router-dom'
import routes from '../../routes'
import { useSite } from './SiteContext'
import SiteProvider from './SiteProvider'
import { useSites } from './SitesContext'

const sites = [
  { id: 'site-1', name: 'site1' },
  { id: 'site-2', name: 'site2' },
  { id: 'site-3', name: 'site3' },
  { id: 'site-4', name: 'site4' },
]

function SiteTest() {
  const site = useSite()
  const sitesContext = useSites()
  const history = useHistory()

  return (
    <>
      <div>{`Current Site: ${site.name}`}</div>
      {sitesContext.map((s) => (
        <Button
          key={s.name}
          onClick={() => history.push(routes.portfolio_reports__siteId(s.id))}
        >
          {`Click on: ${s.name}`}
        </Button>
      ))}
      <Button onClick={() => history.push(routes.timeSeries)}>
        go to time series
      </Button>
      <Button onClick={() => history.push(routes.portfolio_twins_results)}>
        go to twins
      </Button>
      <Button onClick={() => history.push(routes.home)}>
        go to Portfolio Dashboards
      </Button>
      <Button onClick={() => history.push(routes.portfolio_reports)}>
        go to Portfolio Reports
      </Button>
    </>
  )
}

function UserProvider({
  children,
  isPortfolioUser,
  savedId,
  favoriteSiteId,
  saveLocalOptions,
}) {
  const [user, setUser] = useState({
    isPortfolioUser,
    options: {
      siteId: savedId,
      favoriteSiteId,
    },
  })
  const value = {
    ...user,
    saveLocalOptions,
    saveOptions: (k, v) => {
      setUser((prevUser) => ({
        ...prevUser,
        options: {
          ...prevUser.options,
          [k]: v,
        },
      }))
    },
  }

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
}

const Wrapper = ({
  history,
  initialSites,
  savedId,
  favoriteSiteId,
  isPortfolioUser,
  initializeSiteContext,
  saveLocalOptions,
  scopeLocation,
  isScopeSelectorEnabled,
}) => (
  <BaseWrapper
    sites={initialSites}
    scopeLocation={scopeLocation}
    isScopeSelectorEnabled={isScopeSelectorEnabled}
  >
    <UserProvider
      portfolios={[{ id: '152b987f-0da2-4e77-9744-0e5c52f6ff3d' }]}
      showPortfolioTab={isPortfolioUser}
      savedId={savedId}
      favoriteSiteId={favoriteSiteId}
      saveLocalOptions={saveLocalOptions}
    >
      <AnalyticsProvider
        options={{
          initializeSiteContext,
        }}
      >
        <Router history={history}>
          <SiteProvider>
            <SiteTest />
          </SiteProvider>
        </Router>
      </AnalyticsProvider>
    </UserProvider>
  </BaseWrapper>
)

beforeEach(() => jest.clearAllMocks())

describe('SiteProvider', () => {
  const initializeWrapper = ({
    history,
    initialSites = sites,
    savedId,
    saveOptions = () => {},
    saveLocalOptions = () => {},
    favoriteSiteId,
    isPortfolioUser = true,
    initializeSiteContext = (_siteContext) => {},
    isScopeSelectorEnabled,
    scopeLocation,
  }) =>
    render(
      <Wrapper
        history={history}
        initialSites={initialSites}
        savedId={savedId}
        saveOptions={saveOptions}
        saveLocalOptions={saveLocalOptions}
        favoriteSiteId={favoriteSiteId}
        isPortfolioUser={isPortfolioUser}
        initializeSiteContext={initializeSiteContext}
        isScopeSelectorEnabled={isScopeSelectorEnabled}
        scopeLocation={scopeLocation}
      />
    )

  test('when portfolio level user lands on a page where there is siteId matched in url, site context will be the site with matched siteId from url', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: [routes.portfolio_reports__siteId('site-2')],
      }),
      savedId: 'site-3',
      favoriteSiteId: 'site-1',
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site2')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-2', name: 'site2' })
  })

  test('when portfolio level user lands on a page and no siteId matched in url, and user.options.siteId does exist, then site context will be that site', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: [routes.portfolio],
      }),
      savedId: 'site-3',
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-3', name: 'site3' })
  })

  test('when portfolio level user lands on a page, no siteId matched in url, no user.options.siteId, then site context is the first site in sites list', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: [routes.portfolio],
      }),
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site1')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-1', name: 'site1' })
  })

  test('when site level user lands on a page where there is siteId matched in url, site context will be the site with matched siteId from url', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: [routes.portfolio_reports__siteId('site-3')],
      }),
      isPortfolioUser: false,
      savedId: 'site-1',
      favoriteSiteId: 'site-2',
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-3', name: 'site3' })
  })

  test('when site level user logs in and favorite site exist, site context is set to favorite site', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['sites/'],
      }),
      isPortfolioUser: false,
      savedId: 'site-1',
      favoriteSiteId: 'site-2',
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site2')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-2', name: 'site2' })

    await act(async () => {
      userEvent.click(await findByText('Click on: site3'))
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-3', name: 'site3' })
  })

  test('when site level user logs in and favorite site does not exist but user.options.siteId exist, site context is set to site with id of user.options.siteId', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['sites/'],
      }),
      isPortfolioUser: false,
      savedId: 'site-3',
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-3', name: 'site3' })
  })

  test('siteId based on scope coming from scopeSelector overwrites original siteId when scope is a building', async () => {
    const originalSiteId = 'site-3'
    const siteIdBasedOnScope = 'site-1'

    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['sites'],
      }),
      isPortfolioUser: false,
      savedId: originalSiteId,
      initializeSiteContext: mockFunc,
      isScopeSelectorEnabled: true,
      scopeLocation: {
        twin: {
          id: 'twinId-1',
          siteId: siteIdBasedOnScope,
          metadata: {
            modelId: buildingModelId,
          },
        },
      },
    })

    const siteName = sites.find((s) => s.id === siteIdBasedOnScope).name
    expect(await findByText(`Current Site: ${siteName}`)).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: siteIdBasedOnScope, name: siteName })
  })

  test('when site level user logs in and favorite site does not exist and user.options.siteId does not exist, then site context is the first site in sites list', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['/sites/'],
      }),
      isPortfolioUser: false,
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site1')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-1', name: 'site1' })

    await act(async () => {
      userEvent.click(await findByText('Click on: site2'))
    })

    expect(await findByText('Current Site: site2')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-2', name: 'site2' })
  })

  test('savedId will be preserved when use go to other part of the application', async () => {
    const mockFunc = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['/sites/site-3'],
      }),
      isPortfolioUser: false,
      initializeSiteContext: mockFunc,
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
    expect(mockFunc).toBeCalledWith({ id: 'site-3', name: 'site3' })

    await act(async () => {
      userEvent.click(await findByText('go to time series'))
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()

    await act(async () => {
      userEvent.click(await findByText('go to twins'))
    })

    expect(await findByText('Current Site: site3')).toBeInTheDocument()
  })

  test('when current pathname matches sites/:siteId or pathname starts with routes.home, should fire user.saveLocalOptions', async () => {
    const mockSaveLocalOptions = jest.fn()
    const { findByText } = initializeWrapper({
      history: createMemoryHistory({
        initialEntries: ['/sites/site-4'],
      }),
      isPortfolioUser: false,
      saveLocalOptions: mockSaveLocalOptions,
    })

    expect(mockSaveLocalOptions).toBeCalledWith('lastSelectedSiteId', 'site-4')

    await act(async () => {
      userEvent.click(await findByText('Click on: site2'))
    })

    expect(mockSaveLocalOptions).toBeCalledWith('lastSelectedSiteId', 'site-2')

    await act(async () => {
      userEvent.click(await findByText('go to Portfolio Dashboards'))
    })

    expect(mockSaveLocalOptions).toBeCalledWith('lastSelectedSiteId', null)

    await act(async () => {
      userEvent.click(await findByText('go to Portfolio Reports'))
    })

    expect(mockSaveLocalOptions).toBeCalledWith('lastSelectedSiteId', null)
  })
})

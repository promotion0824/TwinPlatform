import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import * as modelOfInterest from '@willow/common/twins/view/modelsOfInterest'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { supportDropdowns } from '@willow/ui/utils/testUtils/dropdown'
import { useState } from 'react'
import { Route, useParams } from 'react-router'

import { PortfolioContext } from '../PortfolioContext'
import Sites from './Sites'
import PagedPortfolioProvider from './PagedPortfolioProvider'

// <SiteChip /> in KpiSite is not relevant to this test file,
// so mock useModelsOfInterest hook will remove console.error log
const mockModelOfInterestData = { isSuccess: false }
modelOfInterest.useModelsOfInterest = jest.fn(() => mockModelOfInterestData)

supportDropdowns()

describe('Sites', () => {
  beforeEach(() => {
    Element.prototype.getBoundingClientRect = jest.fn(() => ({
      width: 400,
      height: 200,
      top: 0,
      left: 0,
      bottom: 0,
      right: 0,
      x: 0,
      y: 0,
      toJSON: () => {},
    }))
  })
  test('expect to see sites and click on single site will navigate to site dashboard', async () => {
    render(<Sites />, {
      wrapper: Wrapper,
    })

    await checkSitesStatus(sites, undefined)

    const siteOneContainer = await screen.findByTestId(
      `site-${siteOneHasScore.id}`
    )

    userEvent.click(siteOneContainer)
    assertOnSiteDashboard(siteOneHasScore.id)
  })
})

const assertOnSiteDashboard = async (siteId) =>
  expect(
    await screen.findByText(`Site Dashboard for ${siteId}`)
  ).toBeInTheDocument()

const checkSitesStatus = async (sites, selectedSite) => {
  for (const site of sites) {
    const siteContainer = await screen.findByTestId(`site-${site.id}`)
    expect(siteContainer).toBeInTheDocument()
    expect(siteContainer).toHaveStyle({
      background: site.id === selectedSite?.id ? '#2c2c2c' : '#242424',
    })
  }
}

const siteOneHasScore = {
  id: 'id-1',
  name: '347 Kent Street',
  type: 'type-1',
  status: 'status-1',
}
const siteTwoHasNoScore = {
  id: 'id-2',
  name: '120 Collins Street',
  type: 'type-2',
  status: 'status-2',
}

const sites = [siteOneHasScore, siteTwoHasNoScore]

function Wrapper({ children }) {
  return (
    <BaseWrapper initialEntries={['/portfolio/dashboards']}>
      <Route exact path="/portfolio/dashboards">
        <PortfolioProvider>{children}</PortfolioProvider>
      </Route>
      <Route exact path="/sites/:siteId">
        <Dashboard />
      </Route>
    </BaseWrapper>
  )
}

const Dashboard = () => {
  const { siteId } = useParams()
  return <div>`Dashboard for ${siteId}`</div>
}

function PortfolioProvider({ children }) {
  const [selectedSite, setSelectedSite] = useState()

  const value = {
    baseMapSites: sites,
    filteredSites: sites,
    selectedSite,
    selectSite: setSelectedSite,
    isBuildingScoresLoading: false,
    buildingScores: [
      {
        siteId: siteOneHasScore.id,
        score: 0.39,
      },
      {
        siteId: siteTwoHasNoScore.id,
      },
    ],
  }

  return (
    <PagedPortfolioProvider>
      <PortfolioContext.Provider value={value}>
        {children}
      </PortfolioContext.Provider>
    </PagedPortfolioProvider>
  )
}

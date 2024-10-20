import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import '@willow/common/utils/testUtils/matchMediaMock'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { ScopeSelectorProvider } from '@willow/ui'
import {
  openDropdown,
  supportDropdowns,
} from '@willow/ui/utils/testUtils/dropdown'
import withTranslationValues from '@willow/ui/utils/withTranslationValues'
import { createMemoryHistory } from 'history'
import { act } from 'react-dom/test-utils'
import { Route, Switch } from 'react-router'
import { Router } from 'react-router-dom'
import SiteProvider from '../../providers/sites/SiteProvider'
import routes from '../../routes'
import LayoutProvider from '../Layout/Layout/Layout'
import CommandLayoutHeader from './CommandLayoutHeader'

supportDropdowns()

describe('SiteSelect', () => {
  const getDropdownButton = () => {
    const buttons = screen.getAllByRole('button')
    return buttons.find((b) => b.className.includes('dropdownHeaderButton'))
  }

  test.each([
    {
      pathname: routes.sites__siteId_insights('id-abc-1'),
      nextSiteName: 'Building Two',
      expectedPathname: routes.sites__siteId_insights('id-abd-2'),
    },
    {
      pathname: routes.sites__siteId_tickets('id-abd-2'),
      nextSiteName: 'Building Three',
      expectedPathname: routes.sites__siteId_tickets('id-cde-3'),
    },
    {
      pathname: routes.sites__siteId_inspections('id-cde-3'),
      nextSiteName: 'Building Four',
      expectedPathname: routes.sites__siteId_inspections('id-efg-4'),
    },
    {
      pathname: routes.sites__siteId_occupancy('id-qwd-5'),
      nextSiteName: 'Building Six',
      expectedPathname: routes.sites__siteId_occupancy('id-iop-6'),
    },
    {
      pathname: routes.sites__siteId_reports('id-iop-6'),
      nextSiteName: 'Building Seven',
      expectedPathname: routes.sites__siteId_reports('id-cbn-7'),
    },
  ])(
    'when current pathname is a site page and branch is defined in routes.branches, click on new site option should push history to $expectedPathname',
    async ({ pathname, nextSiteName, expectedPathname }) => {
      const history = createMemoryHistory({
        initialEntries: [pathname],
      })

      const { findByText } = render(<Wrapper history={history} />)
      const dropdownButton = getDropdownButton()

      openDropdown(dropdownButton)
      const siteOption = await findByText(nextSiteName)
      act(() => {
        userEvent.click(siteOption)
      })

      expect(history.location.pathname).toBe(expectedPathname)
    }
  )

  test('when pathname is a site page but branch DOES NOT exist or not defined in branches, push new pathname of routes.sites__siteId(newSiteId) to history', async () => {
    const history = createMemoryHistory({
      initialEntries: [`${routes.sites__siteId(siteFive.id)}?admin=true`],
    })

    const { findByText } = render(<Wrapper history={history} />)
    const dropdownButton = getDropdownButton()

    openDropdown(dropdownButton)
    const siteTwoOption = await findByText(siteTwo.name)
    act(() => {
      userEvent.click(siteTwoOption)
    })

    expect(history.location.pathname).toBe(routes.sites__siteId(siteTwo.id))

    /* simulate user landing on sites__siteId_floors page page for site two */
    act(() => {
      history.push(
        routes.sites__siteId_floors__floorId(siteTwo.id, '123456floorid')
      )
    })

    openDropdown(dropdownButton)
    const siteThreeOption = await findByText(siteThree.name)
    act(() => {
      userEvent.click(siteThreeOption)
    })

    expect(history.location.pathname).toBe(routes.sites__siteId(siteThree.id))
  })

  test('expect search params to persist when user clicks on new site option', async () => {
    const defaultParamsString =
      '?category=DataQuality&admin=true&view=performanceView'

    const history = createMemoryHistory({
      initialEntries: [
        `${routes.sites__siteId_tickets(siteThree.id)}${defaultParamsString}`,
      ],
    })

    const { findByText } = render(
      <Wrapper history={history} isReadOnly={false} />
    )
    const dropdownButton = getDropdownButton()

    openDropdown(dropdownButton)
    const siteSixOption = await findByText(siteSix.name)
    act(() => {
      userEvent.click(siteSixOption)
    })

    expect(history.location.pathname).toBe(
      routes.sites__siteId_tickets(siteSix.id)
    )
    expect(history.location.search).toBe(defaultParamsString)
  })
})

const features = {
  isInsightsDisabled: true,
  isTicketingDisabled: true,
  isInspectionEnabled: false,
  isReportsEnabled: false,
  isOccupancyEnabled: false,
  userRole: 'user',
}

const siteOne = {
  id: 'id-abc-1',
  name: 'Building One',
  features,
}

const siteTwo = {
  id: 'id-abd-2',
  name: 'Building Two',
  features,
}

const siteThree = {
  id: 'id-cde-3',
  name: 'Building Three',
  features,
  userRole: 'admin',
}

const siteFour = {
  id: 'id-efg-4',
  name: 'Building Four',
  features,
}

const siteFive = {
  id: 'id-qwd-5',
  name: 'Building Five',
  features,
}

const siteSix = {
  id: 'id-iop-6',
  name: 'Building Six',
  features,
}

const siteSeven = {
  id: 'id-cbn-7',
  name: 'Building Seven',
  features,
}

const sites = [
  siteOne,
  siteTwo,
  siteThree,
  siteFour,
  siteFive,
  siteSix,
  siteSeven,
]

const TranslatedCommandLayoutHeader = withTranslationValues({
  'headers.allSites': 'All sites',
})(CommandLayoutHeader)

const Wrapper = ({ history, showPortfolioTab, isReadOnly = true }) => (
  <BaseWrapper
    user={{
      showPortfolioTab,
      portfolios: [{ id: '152b987f-0da2-4e77-9744-0e5c52f6ff3d' }],
      options: {
        siteId: null,
      },
    }}
    hasFeatureToggle={() => false}
    sites={sites}
  >
    <Router history={history}>
      <ScopeSelectorProvider>
        <SiteProvider>
          <LayoutProvider>
            <Switch>
              <Route>
                <TranslatedCommandLayoutHeader isReadOnly={isReadOnly} />
              </Route>
            </Switch>
          </LayoutProvider>
        </SiteProvider>
      </ScopeSelectorProvider>
    </Router>
  </BaseWrapper>
)

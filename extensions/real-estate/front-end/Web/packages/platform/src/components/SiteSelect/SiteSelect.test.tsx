import { History } from 'history'
import { Route, Switch } from 'react-router'
import { Router } from 'react-router-dom'
import { act } from 'react-dom/test-utils'
import { createMemoryHistory } from 'history'
import { screen, render } from '@testing-library/react'
import OnClickOutsideIdsStubProvider from '@willow/ui/providers/OnClickOutsideIdsProvider/OnClickOutsideIdsStubProvider'
import UserStubProvider from '@willow/ui/providers/UserProvider/UserStubProvider'
import withTranslationValues from '@willow/ui/utils/withTranslationValues'
import {
  supportDropdowns,
  openDropdown,
} from '@willow/ui/utils/testUtils/dropdown'
import getSiteIdFromUrl from '@willow/ui/utils/getSiteIdFromUrl'
import userEvent from '@testing-library/user-event'
import { ALL_SITES } from '@willow/ui'
import { AllSites } from '../../providers/sites/SiteContext'
import SiteSelect from './SiteSelect'
import routes from '../../routes'
import { Site } from '@willow/common/site/site/types'

supportDropdowns()

describe('SiteSelect', () => {
  test('should trigger handleSiteChange when a site option is clicked, should trigger user.saveOptions when a favorite site button is clicked', async () => {
    const history = createMemoryHistory({
      initialEntries: [routes.sites__siteId(siteOne.id)],
    })

    const mockedHandleSiteChange = jest.fn()
    const mockedSaveFavoriteSite = jest.fn()

    const { findByText } = render(
      <Wrapper
        history={history}
        sites={sites}
        value={
          sites.find(
            (s) => s.id === getSiteIdFromUrl(history.location.pathname)
          ) ?? { id: null, name: ALL_SITES }
        }
        handleSiteChange={mockedHandleSiteChange}
        saveOptions={mockedSaveFavoriteSite}
      />
    )

    // currently selected site from initialEntries
    expect(await findByText(siteOne.name)).toBeInTheDocument()

    openDropdown(screen.getByRole('button'))

    const siteOptions = screen
      .getAllByRole('button')
      .filter((optionButton) => optionButton.className.includes('option'))
    /* there will be 4 options, 3 sites plus "All Sites" */
    expect(siteOptions.length).toBe(4)

    const buildingTwo = await findByText(siteTwo.name)
    act(() => {
      userEvent.click(buildingTwo)
    })

    expect(mockedHandleSiteChange).toBeCalledWith(siteTwo)

    openDropdown(screen.getByRole('button'))
    const siteOptionButtons = screen.getAllByRole('button')
    const siteFavoriteButtons = siteOptionButtons.filter((b) =>
      b.className.includes('siteFavoriteButton')
    )
    act(() => {
      // siteThree is at position 2
      userEvent.click(siteFavoriteButtons[2])
    })

    expect(mockedSaveFavoriteSite).toBeCalledWith(
      'favoriteSiteId',
      siteThree.id
    )
  })

  test('click on a site should navigate to corresponding pathname defined by handleSiteChange', async () => {
    const history = createMemoryHistory({
      initialEntries: [routes.sites__siteId(siteThree.id)],
    })

    const handleSiteChange = (site: Site) => {
      history.push(
        site?.id != null ? routes.sites__siteId(site.id) : routes.home
      )
    }

    const { findByText } = render(
      <Wrapper
        history={history}
        sites={sites}
        value={
          sites.find(
            (s) => s.id === getSiteIdFromUrl(history.location.pathname)
          ) ?? { id: null, name: ALL_SITES }
        }
        handleSiteChange={handleSiteChange}
        saveOptions={jest.fn()}
      />
    )

    openDropdown(screen.getByRole('button'))
    const buildingOne = await findByText(siteOne.name)
    act(() => {
      userEvent.click(buildingOne)
    })

    expect(history.location.pathname).toBe(routes.sites__siteId(siteOne.id))

    openDropdown(screen.getByRole('button'))
    const buildingTwo = await findByText(siteTwo.name)
    act(() => {
      userEvent.click(buildingTwo)
    })

    expect(history.location.pathname).toBe(routes.sites__siteId(siteTwo.id))

    openDropdown(screen.getByRole('button'))
    const allSite = await findByText(ALL_SITES)
    act(() => {
      userEvent.click(allSite)
    })

    expect(history.location.pathname).toBe(routes.home)
  })
})

const siteOne = {
  id: 'id-1',
  name: 'Building One',
}
const siteTwo = {
  id: 'id-2',
  name: 'Building Two',
}
const siteThree = {
  id: 'id-3',
  name: 'Building Three',
}

const sites = [siteOne, siteTwo, siteThree] as Site[]

const TranslatedSitesSelect = withTranslationValues({
  'headers.allSites': ALL_SITES,
})(SiteSelect)

const Wrapper = ({
  value,
  sites,
  history,
  handleSiteChange,
  saveOptions = () => {},
}: {
  value: Site | AllSites
  sites: Site[]
  history: History
  handleSiteChange: (site: Site) => void
  saveOptions: () => void
}) => (
  <Router history={history}>
    <Switch>
      <Route>
        <UserStubProvider options={{ saveOptions }}>
          <OnClickOutsideIdsStubProvider>
            <TranslatedSitesSelect
              onChange={handleSiteChange}
              sites={sites}
              history={history}
              value={value}
            />
          </OnClickOutsideIdsStubProvider>
        </UserStubProvider>
      </Route>
    </Switch>
  </Router>
)

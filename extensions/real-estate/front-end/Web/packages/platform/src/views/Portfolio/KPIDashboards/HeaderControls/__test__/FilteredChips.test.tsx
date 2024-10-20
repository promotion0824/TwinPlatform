import { render } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { act } from 'react-dom/test-utils'
import { usePortfolio } from '../../../PortfolioContext'
import FilteredChips from '../FilteredChips'
import getMockPortfolio, {
  fiftyMartin,
  fortyMartin,
  sixtyMartin,
  thirtyMartin,
} from './utils'

jest.mock('../../../PortfolioContext')
const mockedUsePortfolio = jest.mocked(usePortfolio)
afterEach(() => {
  mockedUsePortfolio.mockReset()
})

describe('FilteredChips.tsx', () => {
  test('every chip should be visible when condition is met, and corresponding handler function should be called when clicked', async () => {
    const {
      mockedSelectSite,
      mockedToggleBuilding,
      mockedToggleLocation,
      mockedToggleType,
      mockedToggleStatus,
      portfolio,
    } = getMockPortfolio({})
    mockedUsePortfolio.mockImplementation(() => portfolio)
    const { findByText } = render(<FilteredChips />, {
      wrapper: TranslatedWrapper,
    })

    const sixtyMartinChip = await findByText('60 Martin Street')
    const brisbaneChip = await findByText('Brisbane')
    const aviationChip = await findByText('Aviation')
    const constructionChip = await findByText('Construction')

    expect(sixtyMartinChip).toBeInTheDocument()
    expect(brisbaneChip).toBeInTheDocument()
    expect(aviationChip).toBeInTheDocument()
    expect(constructionChip).toBeInTheDocument()

    act(() => {
      userEvent.click(sixtyMartinChip)
    })
    expect(mockedSelectSite).toBeCalled()
    expect(mockedToggleBuilding).toBeCalledWith(sixtyMartin)

    act(() => {
      userEvent.click(brisbaneChip)
    })
    expect(mockedToggleLocation).toBeCalledWith('Worldwide')

    act(() => {
      userEvent.click(aviationChip)
    })
    expect(mockedToggleType).toBeCalledWith('All Building Types')

    act(() => {
      userEvent.click(constructionChip)
    })
    expect(mockedToggleStatus).toBeCalledWith('All Status')
  })

  test('None of the chip should be visible when conditions are not met', async () => {
    const { portfolio } = getMockPortfolio({
      selectedBuilding: { id: null, name: 'All Sites' },
      selectedLocation: [],
      selectedTypes: ['All Building Types'],
      selectedStatuses: ['All Status'],
      filteredSiteIds: [
        sixtyMartin.id,
        fiftyMartin.id,
        fortyMartin.id,
        thirtyMartin.id,
      ],
    })
    mockedUsePortfolio.mockImplementation(() => portfolio)
    const { container } = render(<FilteredChips />, {
      wrapper: TranslatedWrapper,
    })

    expect(container.querySelectorAll('div[font-size="1"]').length).toBe(0)
  })
})

const TranslatedWrapper = (props) => (
  <BaseWrapper
    {...props}
    translation={{
      'interpolation.buildingTypes': '{{num}} Building Types',
      'interpolation.plainText': '$t(plainText.{{key}})',
      'plainText.aviation': 'Aviation',
      'interpolation.status': '{{num}} Status',
      'plainText.construction': 'Construction',
    }}
  />
)

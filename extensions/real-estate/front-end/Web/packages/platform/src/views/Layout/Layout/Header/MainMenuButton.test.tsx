import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { assertPathnameContains } from '@willow/common/utils/testUtils/LocationDisplay'
import { useAnalytics } from '@willow/ui/providers/AnalyticsProvider/AnalyticsContext'
import { useUser } from '@willow/ui/providers/UserProvider/UserContext'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { noop } from 'lodash'
import MainMenuButton from './MainMenuButton'

jest.mock('@willow/ui/providers/AnalyticsProvider/AnalyticsContext')
const mockUseAnalytics = jest.mocked(useAnalytics)
jest.mock('@willow/ui/providers/UserProvider/UserContext')
const mockUseUser = jest.mocked(useUser)

afterEach(() => {
  jest.resetAllMocks()
})

describe('MainMenuButton', () => {
  const defaultHeader = 'header'

  test('Display a button when header & to props are given', () => {
    render(<MainMenuButton header={defaultHeader} onClick={noop} />)

    expect(screen.getByRole('button')).toBeInTheDocument()
    expect(screen.getByText(defaultHeader)).toBeInTheDocument()
  })

  test('Display one uppercase tile when header is a single word', () => {
    render(<MainMenuButton header={defaultHeader} onClick={noop} />)

    const firstLetter = defaultHeader[0].toUpperCase()
    expect(screen.getByText(firstLetter)).toBeInTheDocument()
  })

  test('Display a multiple uppercase tile when header is a multiple word', () => {
    const header = `${defaultHeader} menu`
    render(<MainMenuButton header={header} onClick={noop} />)

    const initialLetters = header
      .split(' ')
      .map((word) => word[0])
      .join('')
      .toUpperCase()
    expect(screen.getByText(initialLetters)).toBeInTheDocument()
  })

  test('Display a default tile when defaultTile is given', () => {
    const defaultTile = 'DT'
    render(
      <MainMenuButton
        header={defaultHeader}
        defaultTile={defaultTile}
        onClick={noop}
      />
    )

    expect(screen.getByText(defaultTile)).toBeInTheDocument()
  })

  test('click on main menu button should navigate user to new path and fire handlers', async () => {
    const newPath = '/newPath'
    const customerName = 'customer-1'
    const mockOnClick = jest.fn()
    const mockTrack = jest.fn()
    mockUseUser.mockImplementation(() => ({
      customer: { name: customerName },
    }))
    mockUseAnalytics.mockImplementation(() => ({
      track: mockTrack,
    }))

    render(
      <MainMenuButton
        header={defaultHeader}
        to={newPath}
        onClick={mockOnClick}
      />,
      {
        wrapper: BaseWrapper,
      }
    )

    // Main Menu Button is visible
    const menuButton = await screen.findByText(defaultHeader)
    expect(menuButton).toBeInTheDocument()

    // click on MainMenuButton
    await act(async () => {
      userEvent.click(menuButton)
    })

    // expect onClick and track to be fired
    expect(mockOnClick).toBeCalled()
    expect(mockTrack).toBeCalledWith('Main Menu Clicked', {
      main_menu_button_name: defaultHeader,
      customer_name: customerName,
    })

    // expect to navigate to new path
    await waitFor(() => {
      assertPathnameContains(newPath)
    })
  })
})

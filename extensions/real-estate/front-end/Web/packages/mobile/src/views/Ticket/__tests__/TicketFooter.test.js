import { render, screen, waitFor } from '@testing-library/react'
import { v4 as uuidv4 } from 'uuid'
import { TicketStatusesStubProvider } from '@willow/common'
import {
  openDropdown,
  supportDropdowns,
} from '@willow/common/utils/testUtils/dropdown'
import { Status, Tab } from '@willow/common/ticketStatus'
import TicketFooter from '../TicketFooter'
import BaseWrapper from '../../../utils/testUtils/Wrapper.tsx'
import { TICKET_STATUS_DISPLAY_NAMES } from '../../../utils/ticketStatus'

const mockedOnStatusChange = jest.fn()
const mockedOnReassignTicket = jest.fn()

const customerId = uuidv4()

const basicStatuses = [
  {
    customerId,
    status: Status.open,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 1,
  },
  {
    customerId,
    status: Status.inProgress,
    color: 'green',
    tab: Tab.open,
    statusCode: 2,
  },
  {
    customerId,
    status: Status.limitedAvailability,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 3,
  },
  {
    customerId,
    status: Status.reassign,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 4,
  },
  {
    customerId,
    status: Status.resolved,
    color: 'green',
    tab: Tab.resolved,
    statusCode: 5,
  },
  {
    customerId,
    status: Status.closed,
    color: 'green',
    tab: Tab.closed,
    statusCode: 6,
  },
]

const basicStatusesWithOnHold = [
  ...basicStatuses,
  {
    customerId,
    status: Status.onHold,
    color: 'green',
    tab: Tab.open,
    statusCode: 10,
  },
]

supportDropdowns()

const getWrapper = (mockedTicketStatuses) =>
  function Wrapper({ children }) {
    return (
      <BaseWrapper user={{ customerId: '12345' }}>
        <TicketStatusesStubProvider data={mockedTicketStatuses}>
          {children}
        </TicketStatusesStubProvider>
      </BaseWrapper>
    )
  }

const setup = (mockedStatuses, status, ticketType, assigneeType = '') => {
  render(
    <TicketFooter
      ticket={{
        priority: 1,
        statusCode: mockedStatuses.find(
          (ticketStatus) => ticketStatus.status === status
        ).statusCode,
        assigneeType,
      }}
      ticketType={ticketType}
      onStatusChange={mockedOnStatusChange}
      onReassignTicket={mockedOnReassignTicket}
    />,
    { wrapper: getWrapper(mockedStatuses) }
  )
}

afterAll(() => {
  mockedOnStatusChange.mockReset()
  mockedOnReassignTicket.mockReset()
})

describe('TicketFooter', () => {
  describe('Standard ticket with basic statuses', () => {
    test.each([
      {
        status: Status.open,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'workGroup',
      },
      {
        status: Status.open,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'noAssignee',
      },
      {
        status: Status.open,
        expectedButtons: ['Reject', 'Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
      {
        status: Status.inProgress,
        expectedButtons: ['Partially Fixed', 'Completed', 'Reassign'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.limitedAvailability,
        expectedButtons: ['Completed', 'Reassign'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.reassign,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
    ])(
      'Ticket with status [$status] and assigneeType [$assigneeType] should show the correct buttons',
      async ({ status, expectedButtons, isDropdown, assigneeType }) => {
        setup(basicStatuses, status, 'standard', assigneeType)

        await waitFor(() =>
          expect(
            screen.queryByRole('img', { name: 'loading' })
          ).not.toBeInTheDocument()
        )

        if (isDropdown) {
          const button = screen.getByRole('button', {
            name: TICKET_STATUS_DISPLAY_NAMES[status],
          })
          openDropdown(button)

          const optionButtons = await screen.findAllByRole('option')
          assertButtons(optionButtons, expectedButtons)
        } else {
          const buttons = screen.queryAllByRole('button')
          assertButtons(buttons, expectedButtons)
        }
      }
    )

    test.each([
      {
        status: Status.resolved,
      },
      {
        status: Status.closed,
      },
    ])(
      'Ticket with status [$status] should show no button',
      async ({ status, assigneeType }) => {
        setup(basicStatuses, status, 'standard', assigneeType)

        expect(screen.queryByRole('button')).not.toBeInTheDocument()
      }
    )
  })

  describe('Standard ticket with additional "On hold" status', () => {
    test.each([
      {
        status: Status.open,
        expectedButtons: ['On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'workGroup',
      },
      {
        status: Status.open,
        expectedButtons: ['On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'noAssignee',
      },
      {
        status: Status.open,
        expectedButtons: ['Reject', 'On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
      {
        status: Status.inProgress,
        expectedButtons: [
          'Partially Fixed',
          'On Hold',
          'Completed',
          'Reassign',
        ],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.onHold,
        expectedButtons: ['Open', 'In Progress'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.limitedAvailability,
        expectedButtons: ['Completed', 'Reassign'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.reassign,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
    ])(
      'Ticket with status [$status] and assigneeType [$assigneeType] should show the correct buttons',
      async ({ status, expectedButtons, isDropdown, assigneeType }) => {
        setup(basicStatusesWithOnHold, status, 'standard', assigneeType)

        await waitFor(() =>
          expect(
            screen.queryByRole('img', { name: 'loading' })
          ).not.toBeInTheDocument()
        )

        if (isDropdown) {
          const button = screen.getByRole('button', {
            name: TICKET_STATUS_DISPLAY_NAMES[status],
          })
          openDropdown(button)

          const optionButtons = await screen.findAllByRole('option')
          assertButtons(optionButtons, expectedButtons)
        } else {
          const buttons = screen.queryAllByRole('button')
          assertButtons(buttons, expectedButtons)
        }
      }
    )

    test.each([
      {
        status: Status.resolved,
      },
      {
        status: Status.closed,
      },
    ])(
      'Ticket with status [$status] should show no button',
      async ({ status, assigneeType }) => {
        setup(basicStatusesWithOnHold, status, 'standard', assigneeType)

        expect(screen.queryByRole('button')).not.toBeInTheDocument()
      }
    )
  })

  describe('Scheduled ticket with basis statuses', () => {
    test.each([
      {
        status: Status.open,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'workGroup',
      },
      {
        status: Status.open,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'noAssignee',
      },
      {
        status: Status.open,
        expectedButtons: ['Reject', 'Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
      {
        status: Status.inProgress,
        expectedButtons: ['Completed'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.limitedAvailability,
        expectedButtons: ['Completed'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.reassign,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
    ])(
      'Ticket with status [$status] and assigneeType [$assigneeType] should show the correct buttons',
      async ({ status, expectedButtons, isDropdown, assigneeType }) => {
        setup(basicStatuses, status, 'scheduled', assigneeType)

        await waitFor(() =>
          expect(
            screen.queryByRole('img', { name: 'loading' })
          ).not.toBeInTheDocument()
        )

        if (isDropdown) {
          const button = screen.getByRole('button', {
            name: TICKET_STATUS_DISPLAY_NAMES[status],
          })
          openDropdown(button)

          const optionButtons = await screen.findAllByRole('option')
          assertButtons(optionButtons, expectedButtons)
        } else {
          const buttons = screen.queryAllByRole('button')
          assertButtons(buttons, expectedButtons)
        }
      }
    )

    test.each([
      {
        status: Status.resolved,
      },
      {
        status: Status.closed,
      },
    ])(
      'Ticket with status [$status] should show no button',
      async ({ status, assigneeType }) => {
        setup(basicStatuses, status, 'scheduled', assigneeType)

        expect(screen.queryByRole('button')).not.toBeInTheDocument()
      }
    )
  })

  describe('Scheduled ticket with additional "On Hold" status', () => {
    test.each([
      {
        status: Status.open,
        expectedButtons: ['On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'workGroup',
      },
      {
        status: Status.open,
        expectedButtons: ['On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'noAssignee',
      },
      {
        status: Status.open,
        expectedButtons: ['Reject', 'On Hold', 'Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
      {
        status: Status.inProgress,
        expectedButtons: ['On Hold', 'Completed'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.onHold,
        expectedButtons: ['Open', 'In Progress'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.limitedAvailability,
        expectedButtons: ['Completed'],
        isDropdown: true,
        assigneeType: 'other',
      },
      {
        status: Status.reassign,
        expectedButtons: ['Accept'],
        isDropdown: false,
        assigneeType: 'other',
      },
    ])(
      'Ticket with status [$status] and assigneeType [$assigneeType] should show the correct buttons',
      async ({ status, expectedButtons, isDropdown, assigneeType }) => {
        setup(basicStatusesWithOnHold, status, 'scheduled', assigneeType)

        await waitFor(() =>
          expect(
            screen.queryByRole('img', { name: 'loading' })
          ).not.toBeInTheDocument()
        )

        if (isDropdown) {
          const button = screen.getByRole('button', {
            name: TICKET_STATUS_DISPLAY_NAMES[status],
          })
          openDropdown(button)

          const optionButtons = await screen.findAllByRole('option')
          assertButtons(optionButtons, expectedButtons)
        } else {
          const buttons = screen.queryAllByRole('button')
          assertButtons(buttons, expectedButtons)
        }
      }
    )

    test.each([
      {
        status: Status.resolved,
      },
      {
        status: Status.closed,
      },
    ])(
      'Ticket with status [$status] should show no button',
      async ({ status, assigneeType }) => {
        setup(basicStatuses, status, 'scheduled', assigneeType)

        expect(screen.queryByRole('button')).not.toBeInTheDocument()
      }
    )
  })
})

const assertButtons = (buttons, expectedButtons) => {
  expect(buttons).toHaveLength(expectedButtons.length)

  buttons.forEach((button, index) => {
    expect(button).toHaveTextContent(expectedButtons[index])
  })
}

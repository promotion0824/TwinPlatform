import { render, screen, within, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { v4 as uuidv4 } from 'uuid'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { Form } from '@willow/ui'
import {
  supportDropdowns,
  openDropdown,
} from '@willow/ui/utils/testUtils/dropdown'
import { TicketStatusesStubProvider } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import TicketStatusSelect from './TicketStatusSelect'

const customerId = uuidv4()

function getWrapper(status, mockedTicketStatuses) {
  return ({ children }) => (
    <BaseWrapper user={{ customer: { id: customerId, name: 'Customer' } }}>
      <TicketStatusesStubProvider data={mockedTicketStatuses}>
        <Form defaultValue={{ status }}>{children}</Form>
      </TicketStatusesStubProvider>
    </BaseWrapper>
  )
}

const basicStatuses = [
  {
    customerId,
    status: Status.open,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 0,
  },
  {
    customerId,
    status: Status.inProgress,
    color: 'green',
    tab: Tab.open,
    statusCode: 10,
  },
  {
    customerId,
    status: Status.limitedAvailability,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 20,
  },
  {
    customerId,
    status: Status.reassign,
    color: 'yellow',
    tab: Tab.open,
    statusCode: 30,
  },
  {
    customerId,
    status: Status.resolved,
    color: 'green',
    tab: Tab.resolved,
    statusCode: 40,
  },
  {
    customerId,
    status: Status.closed,
    color: 'green',
    tab: Tab.closed,
    statusCode: 50,
  },
]

const validateDropdownList = async (expectedOptions: string[]) => {
  openDropdown(screen.getByRole('button'))

  const dropdownContent = await screen.findByTestId('dropdown-content')

  const dropdownButtons = await within(dropdownContent).findAllByRole('button')

  expect(dropdownButtons.length).toBe(expectedOptions.length)
  dropdownButtons.forEach((dropdownButton, index) => {
    expect(dropdownButton.textContent).toMatch(
      new RegExp(expectedOptions[index], 'i')
    )
  })
}

supportDropdowns()

const setup = async (mockedStatuses, status: Status, readOnly?: boolean) => {
  render(
    <TicketStatusSelect
      initialStatusCode={
        mockedStatuses.find((ticketStatus) => ticketStatus.status === status)
          .statusCode
      }
      readOnly={readOnly}
    />,
    {
      wrapper: getWrapper(status, mockedStatuses),
    }
  )

  await waitFor(() =>
    expect(
      screen.queryByRole('img', { name: 'loading' })
    ).not.toBeInTheDocument()
  )
}

describe('TicketStatusSelect', () => {
  describe('Basic statuses', () => {
    test.each(
      basicStatuses.map((ticketStatus) => ({ status: ticketStatus.status }))
    )(
      'Show the correct options in dropdown when selected status is $status',
      async ({ status }) => {
        await setup(basicStatuses, status)

        const expectedOptions = [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ]

        await validateDropdownList(expectedOptions)
      }
    )

    test('Should not open dropdown content in ReadOnly mode', async () => {
      await setup(basicStatuses, Status.open, true)

      openDropdown(screen.getByRole('button'))

      expect(screen.queryByTestId('dropdown-content')).not.toBeInTheDocument()
    })
  })

  describe('with onHold status option', () => {
    test.each([
      {
        status: Status.open,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedOnHoldStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Onhold',
      },
      {
        status: Status.inProgress,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedOnHoldStatus,

          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Completed',
      },
      {
        status: Status.onHold,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedOnHoldStatus,
        ],
        nextStatus: 'Open',
      },
      {
        status: Status.limitedAvailability,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Open',
      },
      {
        status: Status.reassign,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Open',
      },
      {
        status: Status.resolved,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Inprogress',
      },
      {
        status: Status.closed,
        expectedOptions: [
          capitalizedOpenStatus,
          capitalizedInProgressStatus,
          capitalizedLimitedAvailabilityStatus,
          capitalizedReassignStatus,
          capitalizedResolvedStatus,
          capitalizedClosedStatus,
        ],
        nextStatus: 'Limitedavailability',
      },
    ])(
      'Show the correct options in dropdown when original status is $status and after changing status to $nextStatus',
      async ({ status, expectedOptions, nextStatus }) => {
        await setup(
          [
            ...basicStatuses,
            {
              customerId,
              status: Status.onHold,
              color: 'yellow',
              tab: Tab.open,
              statusCode: 100,
            },
          ],
          status
        )

        await validateDropdownList(expectedOptions)

        userEvent.click(
          within(screen.getByTestId('dropdown-content')).getByText(
            new RegExp(nextStatus, 'i')
          )
        )
        expect(screen.queryByTestId('dropdown-content')).not.toBeInTheDocument()

        expect(screen.getByRole('button').textContent).toMatch(
          new RegExp(nextStatus, 'i')
        )

        await validateDropdownList(expectedOptions)
      }
    )
  })
})

const capitalizedOpenStatus = 'Open'
const capitalizedInProgressStatus = 'Inprogress'
const capitalizedLimitedAvailabilityStatus = 'Limitedavailability'
const capitalizedReassignStatus = 'Reassign'
const capitalizedResolvedStatus = 'Completed'
const capitalizedClosedStatus = 'Closed'
const capitalizedOnHoldStatus = 'Onhold'

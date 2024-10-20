import { renderHook, waitFor } from '@testing-library/react'
import {
  TicketStatusesProvider,
  useTicketStatuses,
} from './TicketStatusesProvider'
import { Status, Tab, TicketStatus } from '../../ticketStatus'
import ReactQueryStubProvider from '../ReactQueryProvider/ReactQueryStubProvider'

const getWrapper =
  (
    customerId: string,
    getTicketStatuses: (customerId: string) => Promise<TicketStatus[]>
  ) =>
  ({ children }) =>
    (
      <ReactQueryStubProvider>
        <TicketStatusesProvider
          customerId={customerId}
          getTicketStatuses={getTicketStatuses}
        >
          {children}
        </TicketStatusesProvider>
      </ReactQueryStubProvider>
    )

describe('TicketStatusesProvider', () => {
  test('useTicketStatuses with customerId and getter function', async () => {
    const openStatus = {
      status: Status.open,
      statusCode: 100,
      tab: Tab.closed,
      color: 'orange' as const,
    }
    const { result } = renderHook(() => useTicketStatuses(), {
      wrapper: getWrapper('123', (customerId) =>
        Promise.resolve([
          {
            customerId,
            ...openStatus,
          },
        ])
      ),
    })

    expect(result.current).toStrictEqual({
      data: undefined,
      isLoading: true,
      queryStatus: 'loading',
      getByStatus: expect.any(Function),
      getByStatusCode: expect.any(Function),
      isClosed: expect.any(Function),
    })

    await waitFor(() =>
      expect(result.current).toStrictEqual({
        data: [
          {
            customerId: '123',
            ...openStatus,
          },
        ],
        isLoading: false,
        queryStatus: 'success',
        getByStatus: expect.any(Function),
        getByStatusCode: expect.any(Function),
        isClosed: expect.any(Function),
      })
    )

    expect(result.current.getByStatus(Status.open)).toStrictEqual({
      customerId: '123',
      ...openStatus,
    })

    expect(result.current.getByStatusCode(100)).toStrictEqual({
      customerId: '123',
      ...openStatus,
    })

    expect(result.current.getByStatus(Status.closed)).toBeUndefined()

    expect(result.current.getByStatusCode(101)).toBeUndefined()

    expect(result.current.isClosed(100)).toBe(true)

    expect(result.current.isClosed(101)).toBe(false)
  })
})

import React, {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
} from 'react'
import { QueryStatus, UseQueryResult } from 'react-query'
import useGetTicketStatusesByCustomerId from './useGetTicketStatusesByCustomerId'
import { Status, Tab, TicketStatus } from '../../ticketStatus/types'
import { ProviderRequiredError } from '../../exceptions'

export type TicketStatusesType = Pick<
  UseQueryResult<TicketStatus[]>,
  'data' | 'isLoading'
> & {
  queryStatus: QueryStatus
  getByStatusCode: (statusCode?: number) => TicketStatus | undefined
  isClosed: (statusCode?: number) => boolean
  getByStatus: (status: Status) => TicketStatus | undefined
}

export const TicketStatusesContext = createContext<
  TicketStatusesType | undefined
>(undefined)

export const useTicketStatuses = () => {
  const context = useContext(TicketStatusesContext)
  if (context == null) {
    throw new ProviderRequiredError('TicketStatuses')
  }
  return context
}

export const getTicketStatusByCode = (
  allTicketStatuses: TicketStatus[],
  statusCode: number
) =>
  allTicketStatuses.find(
    (ticketStatus) => ticketStatus.statusCode === statusCode
  )

export const getTicketStatusByStatus = (
  allTicketStatuses: TicketStatus[],
  status: Status
) => allTicketStatuses.find((ticketStatus) => ticketStatus.status === status)

// Check if the ticket is closed based on the tab value
// Customers can map custom status code to closed tab
export const isTicketClosed = (
  allTicketStatuses: TicketStatus[],
  statusCode?: number
) => {
  if (statusCode === undefined) {
    return false
  }
  return allTicketStatuses
    .filter((x) => x.tab === Tab.closed)
    .some((x) => x.statusCode === statusCode)
}
export const TicketStatusesProvider = ({
  customerId,
  getTicketStatuses,
  children,
}: PropsWithChildren<{
  customerId: string
  getTicketStatuses: (cusId: string) => Promise<TicketStatus[]>
}>) => {
  const {
    data,
    isLoading,
    status: queryStatus,
  } = useGetTicketStatusesByCustomerId(customerId, getTicketStatuses)
  const context = useMemo(
    () => ({
      data,
      isLoading,
      queryStatus,
      getByStatusCode: (statusCode: number) =>
        data != null ? getTicketStatusByCode(data, statusCode) : undefined,
      getByStatus: (status: Status) =>
        data != null ? getTicketStatusByStatus(data, status) : undefined,
      isClosed: (statusCode?: number) =>
        data != null ? isTicketClosed(data, statusCode) : false,
    }),
    [data, isLoading, queryStatus]
  )
  return (
    <TicketStatusesContext.Provider value={context}>
      {children}
    </TicketStatusesContext.Provider>
  )
}

import React, { PropsWithChildren, useMemo } from 'react'
import {
  getTicketStatusByCode,
  getTicketStatusByStatus,
  TicketStatusesContext,
  TicketStatusesType,
  isTicketClosed,
} from './TicketStatusesProvider'

const TicketStatusesStubProvider = ({
  children,
  isLoading = false,
  queryStatus = 'success',
  data = [],
}: PropsWithChildren<Partial<TicketStatusesType>>) => {
  const context = useMemo(
    () => ({
      data,
      isLoading,
      queryStatus,
      getByStatus: (status) => getTicketStatusByStatus(data, status),
      getByStatusCode: (statusCode) => getTicketStatusByCode(data, statusCode),
      isClosed: (statusCode) => isTicketClosed(data, statusCode),
    }),
    [queryStatus, isLoading, data]
  )
  return (
    <TicketStatusesContext.Provider value={context}>
      {children}
    </TicketStatusesContext.Provider>
  )
}

export default TicketStatusesStubProvider

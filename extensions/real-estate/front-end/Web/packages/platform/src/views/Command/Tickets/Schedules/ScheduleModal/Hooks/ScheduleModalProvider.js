import { useState } from 'react'
import { ScheduleModalContext } from './ScheduleModalContext'

export { useScheduleModal } from './ScheduleModalContext'

export function ScheduleModalProvider({ children }) {
  const [showPushScheduledTicket, setPushScheduledTicket] = useState(false)
  const [newAssets, setNewAssets] = useState([])
  const [submittedNewAssets, setSubmittedNewAssets] = useState([])
  const [isPushScheduledTickets, setIsPushScheduledTickets] = useState()
  const [isFutureStartDate, setIsFutureStartDate] = useState(false)

  return (
    <ScheduleModalContext.Provider
      value={{
        showPushScheduledTicket,
        setPushScheduledTicket,
        newAssets,
        setNewAssets,
        submittedNewAssets,
        setSubmittedNewAssets,
        isPushScheduledTickets,
        setIsPushScheduledTickets,
        isFutureStartDate,
        setIsFutureStartDate,
      }}
    >
      {children}
    </ScheduleModalContext.Provider>
  )
}

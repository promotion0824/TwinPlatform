import { createContext, useContext, useState } from 'react'

import { useDateTime } from '@willow/ui'
import {
  DateTimeRange,
  getDateTimeRange,
} from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'

export interface PageTitle {
  title: string
  href: string
}
interface InspectionsContextType {
  sharedTimeRange: DateTimeRange | undefined
  setSharedTimeRange: (times: DateTimeRange | undefined) => void
  resetTimeRange: () => void

  pageTitles: PageTitle[]
  setPageTitles: React.Dispatch<React.SetStateAction<PageTitle[] | []>>
}

const InspectionsContext = createContext<InspectionsContextType | null>(null)
export const useInspections = () => {
  const inspectionContext = useContext(InspectionsContext)
  if (inspectionContext == null) {
    throw new Error(`useInspections needs an InspectionsProvider`)
  }
  return inspectionContext
}

export const InspectionsProvider = ({ children }) => {
  const currentTimeRange = useCurrentTimeRange()
  const [sharedTimeRange, setSharedTimeRange] = useState<
    DateTimeRange | undefined
  >(currentTimeRange)
  const resetTimeRange = () => {
    setSharedTimeRange(currentTimeRange)
  }

  const [pageTitles, setPageTitles] = useState<
    { title: string; href: string }[] | []
  >([])

  return (
    <InspectionsContext.Provider
      value={{
        pageTitles,
        setPageTitles,

        sharedTimeRange,
        setSharedTimeRange,
        resetTimeRange,
      }}
    >
      {children}
    </InspectionsContext.Provider>
  )
}

export function useCurrentTimeRange() {
  const dateTime = useDateTime()
  const defaultQuickRange = '7D'
  return getDateTimeRange(dateTime.now(), defaultQuickRange)
}

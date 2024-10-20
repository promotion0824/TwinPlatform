import { useState } from 'react'
import { InspectionsContext } from './InspectionsContext'

export default function InspectionsProvider({ children }) {
  const [inspectionZones, setInspectionZones] = useState(null)

  const context = {
    inspectionZones,
    setInspectionZones,
  }

  return (
    <InspectionsContext.Provider value={context}>
      {children}
    </InspectionsContext.Provider>
  )
}

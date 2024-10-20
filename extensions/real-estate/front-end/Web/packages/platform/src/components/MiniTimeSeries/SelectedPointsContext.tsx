import { ProviderRequiredError } from '@willow/common'
import { createContext, useContext } from 'react'

export type Point = {
  sitePointId: string
  [key: string]: unknown
}

export type ApiParams = {
  start: string
  end: string
  interval: string
}

export type State = {
  points: Point[]
  loadingPointIds: string[]
  pointIds: string[]
  pointColorMap: {
    [key: string]: string | undefined
  }
  params?: ApiParams
}

type SelectedPoints = State & {
  onSelectPoint: (sitePointId: string, isSelected: boolean) => void
  onLoadPoint: (sitePointId: string) => void
  onLoadedPoint: (point) => void
  onRemovePoint: (sitePointId: string) => void
  onUpdateParams: (params: ApiParams) => void
  isLoading: boolean
}

const SelectedPointsContext = createContext<SelectedPoints | undefined>(
  undefined
)

export function useSelectedPoints() {
  const context = useContext(SelectedPointsContext)
  if (!context) {
    throw new ProviderRequiredError('SelectedPoints')
  }
  return context
}

export default SelectedPointsContext

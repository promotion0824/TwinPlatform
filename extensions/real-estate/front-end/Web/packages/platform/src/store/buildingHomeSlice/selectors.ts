import useSlice from '../useSlice'
import { BuildingHomeSlice } from './createBuildingHomeSlice'

// eslint-disable-next-line import/prefer-default-export
export const useBuildingHomeSlice = <U = BuildingHomeSlice>(
  selector?: (state: BuildingHomeSlice) => U
) => useSlice('buildingHomeSlice', selector)

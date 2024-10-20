import { lens, withLenses } from '@dhmk/zustand-lens'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'
import { immer } from 'zustand/middleware/immer'

import createBuildingHomeSlice, {
  BuildingHomeSlice,
} from './buildingHomeSlice/createBuildingHomeSlice'

const useStore = create<{
  buildingHomeSlice: BuildingHomeSlice
}>()(
  devtools(
    immer(
      withLenses({
        buildingHomeSlice: lens<BuildingHomeSlice>(createBuildingHomeSlice),
        // append other slices here
      })
    )
  )
)

export default useStore

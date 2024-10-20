import { Fetch, Flex } from '@willow/ui'
import Floors from './Floors/Floors'
import Report from './Report/Report'

export default function Occupancy() {
  return (
    <Fetch
      url="/api/pilot/occupancy"
      params={{
        hasBaseModule: true,
      }}
    >
      {(floors) => (
        <Flex
          horizontal
          fill="equal"
          height="100%"
          size="small"
          padding="small"
        >
          <Floors
            floors={floors.map((floor) => ({
              ...floor,
              id: floor.floorId,
              name: floor.floorName,
              code: floor.floorCode,
              people: Math.max(floor.runningTotal ?? 0, 0),
              peopleLimit: floor.floorLimit,
            }))}
          />
          <Report />
        </Flex>
      )}
    </Fetch>
  )
}

import { Group, Stack } from '@willowinc/ui'
import { useBuildingHomeSlice } from '../../../store/buildingHomeSlice'
import { DraggableLayoutBoard } from './DraggableColumnLayout'
import LocationWidget from './widgets/Location/LocationWidget'
import WIDGET_CARD_MAP from './widgets/widgetCardMap'

const BuildingHomeDraggableDashboard = () => {
  const { layout, saveLayout, isEditingMode } = useBuildingHomeSlice()

  return (
    <Group
      align="flex-start"
      wrap="nowrap"
      w="100%"
      p="s16"
      pt={0}
      css={({ theme }) => ({
        [`@media screen and (max-width: ${theme.breakpoints.mobile})`]: {
          flexDirection: 'column',

          '> :first-child': {
            width: '100%',
          },
        },
      })}
    >
      <LocationWidget />

      <Stack
        w="100%"
        css={{
          flex: 1,
        }}
      >
        <DraggableLayoutBoard
          cols={{
            1800: 2,
            1200: 1,
          }}
          isEditingMode={isEditingMode}
          data={layout}
          setData={saveLayout}
          componentMap={WIDGET_CARD_MAP}
        />
      </Stack>
    </Group>
  )
}

export default BuildingHomeDraggableDashboard

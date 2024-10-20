import { useFloor } from '../FloorContext'
import Layers from './Content/Layers/Layers'

export default function SidePanel() {
  const floor = useFloor()

  return (!floor.isFloorEditorDisabled || floor.selectedAsset) && <Layers />
}

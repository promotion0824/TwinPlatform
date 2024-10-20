import { useFloor } from '../../../FloorContext'
import Toolbar2D from './Toolbar2dLayers/Toolbar'
import Toolbar3D from './Toolbar3dLayers/Toolbar'

export default function Layers(props) {
  const { closePanel, isHidden } = props
  const floor = useFloor()

  return floor.floorViewType === '2D' ? (
    <Toolbar2D closePanel={closePanel} isHidden={isHidden} />
  ) : floor.floorViewType === '3D' ? (
    <Toolbar3D
      closePanel={closePanel}
      isHidden={isHidden}
      iframeRef={floor.iframeRef}
    />
  ) : null
}

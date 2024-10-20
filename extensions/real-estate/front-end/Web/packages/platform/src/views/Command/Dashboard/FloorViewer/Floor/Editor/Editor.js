import { NotFound } from '@willow/ui'
import { Panel } from '@willowinc/ui'
import { useSite } from 'providers'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../FloorContext'
import EditModeProvider from './EditMode/EditModeProvider'
import Editor3d from './Editor3d/Editor3d'
import EditorContent from './EditorContent'
import { EditorContext } from './EditorContext'
import EditorGIS from './EditorGIS/EditorGIS'
import getClosestPoint from './getClosestPoint'

export default function FloorEditor() {
  const floor = useFloor()
  const site = useSite()
  const { t } = useTranslation()

  const contentRef = useRef()
  const svgRef = useRef()
  const tooltipsRef = useRef()

  const [state, setState] = useState({
    x: 0,
    y: 0,
    zoomLevels: [],
  })

  const context = {
    x: state.x,
    y: state.y,
    zoom: state.zoom,
    zoomLevels: state.zoomLevels,

    contentRef,
    svgRef,
    tooltipsRef,

    setZoomLevels(zoomLevels) {
      setState((prevState) => ({
        ...prevState,
        zoomLevels,
      }))
    },

    move(position) {
      setState((prevState) => ({
        ...prevState,
        ...position,
      }))
    },

    getClosestPoint(point) {
      return getClosestPoint(floor, point)
    },
  }

  const showNonTenancyFloor =
    floor.floorViewType === '2D'
      ? floor.isReadOnly &&
        floor.layerGroups.flatMap((layerGroup) => layerGroup.equipments)
          .length === 0
      : floor.isReadOnly && floor.modules3D.length === 0

  return (
    <EditModeProvider>
      <EditorContext.Provider value={context}>
        <Panel defaultSize={70} id="3d-viewer-editor-panel">
          {showNonTenancyFloor ? (
            <NotFound>{t('plainText.nonTrendedFloors')}</NotFound>
          ) : (
            <>
              {floor.floorViewType === '2D' && <EditorContent />}
              {floor.floorViewType === '3D' && <Editor3d />}
            </>
          )}
        </Panel>
      </EditorContext.Provider>
    </EditModeProvider>
  )
}

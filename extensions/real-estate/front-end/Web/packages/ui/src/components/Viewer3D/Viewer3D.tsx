import { ResizeObserverContainer } from '@willow/common'
import { useConsole } from '@willow/ui/providers/ConsoleProvider/ConsoleContext'
import { Box, Tooltip } from '@willowinc/ui'
import { ReactNode, useEffect, useState } from 'react'
import './assets/forge-viewer.css'
import {
  DbGuidDicts,
  DbPropertyGuidDicts,
  LayerInfo,
  ModelStatus,
  MousePosition,
  Urn,
  UrnIndex,
  UrnIndexModelIdDict,
  ViewerCb,
  ViewerControls,
} from './types'
import {
  getDbIdGuidDict,
  getDbIdPropertyGuidDict,
  getDefaultColor,
  getGuids,
  getLayerName,
  updateTooltipStyles,
} from './utils'
import {
  aggregateSelectionChangedListener,
  mouseMoveListener,
  objectUnderMouseChangedListener,
} from './utils/listeners'
import {
  getDbIdPropertiesDict,
  getHideModelFn,
  getSelectFn,
  getShowModelFn,
  getUpdateLayersByUrnIndexFn,
  getViewerResetFn,
  loadModel,
  updateLayers,
  updateViewerCursor,
  viewerReset,
  viewerSelect,
} from './utils/viewerEvents'

type ClickHandler = {
  clickHandler: {
    clickConfig: { click: { offObject: boolean } }
  }
}

const defaultPriorityColors = {
  1: '#fc2d3b', // red
  2: '#ff6200', // orange
  3: '#ffc11a', // yellow
}

function initialize(viewer: Autodesk.Viewing.Viewer3D & ClickHandler) {
  viewer.setProgressiveRendering(false)
  viewer.setQualityLevel(false, true)
  viewer.prefs.tag('ignore-producer')
  viewer.setGroundShadow(false)
  viewer.hidePoints(true)
  viewer.hideLines(true)
  viewer.setLightPreset(4)
  viewer.setQualityLevel(false, false)
  viewer.setGhosting(true)
  viewer.setGroundShadow(true)
  viewer.setGroundReflection(false)
  viewer.setEnvMapBackground(false)
  viewer.setProgressiveRendering(true)
  viewer.setBackgroundColor(43, 43, 43, 43, 43, 43)
  viewer.navigation.toPerspective()
  viewer.navigation.setReverseZoomDirection(true)
  viewer.setSelectionMode(Autodesk.Viewing.SelectionMode.LAST_OBJECT)
  viewer.clickHandler.clickConfig.click.offObject = false
}

function loadScript(callback: () => void) {
  if (!window.Autodesk) {
    const script = document.createElement('script')
    script.src =
      'https://developer.api.autodesk.com/modelderivative/v2/viewers/7.38.0/viewer3D.min.js'
    document.body.appendChild(script)
    script.onload = () => {
      if (callback) callback()
    }
  }
  if (window.Autodesk && callback) callback()
}

function getViewerControls(
  viewer,
  { urnIndexModelIdDict, dbIdGuidDicts, dbIdPropertyGuidDicts },
  loadModelFn,
  urns,
  loadModelCallbacks,
  onModelLoadChange
) {
  return {
    reset: getViewerResetFn(viewer, viewerReset),
    select: getSelectFn(
      viewer,
      {
        urnIndexModelIdDict,
        dbIdGuidDicts,
        dbIdPropertyGuidDicts,
      },
      viewerSelect
    ),
    showModel: getShowModelFn(
      viewer,
      urnIndexModelIdDict,
      loadModelFn,
      urns,
      loadModelCallbacks,
      onModelLoadChange
    ),
    hideModel: getHideModelFn(viewer, urnIndexModelIdDict),
  }
}

function getInnerViewerControls(
  viewer,
  { urnIndexModelIdDict, dbIdGuidDicts },
  getColor: (priority: number) => THREE.Vector4
) {
  return {
    updateLayersByUrnIndex: getUpdateLayersByUrnIndexFn(
      viewer,
      urnIndexModelIdDict,
      dbIdGuidDicts,
      getColor
    ),
  }
}

interface LoadViewerParams {
  accessToken: string
  urns: Urn[]
  layers: LayerInfo[]
  defaultDisplayUrnIndices: number[]
  viewerCb: ViewerCb
  onMousePositionChange: (position: MousePosition) => void
  onMouseHoverChange: (guid: string, modelIndex: number) => void
  onClick: (e: any) => void
  onModelLoadChange: (urnIndex: UrnIndex, status: ModelStatus) => void
  getColor: (priority: number) => THREE.Vector4
  logger?: any
}

function loadViewer({
  accessToken,
  urns,
  layers,
  defaultDisplayUrnIndices,
  viewerCb,
  onMousePositionChange,
  onMouseHoverChange,
  onClick,
  onModelLoadChange,
  getColor,
  logger,
}: LoadViewerParams) {
  function viewingInitializer() {
    const htmlDiv = document.getElementById('forgeViewer')
    if (htmlDiv == null) {
      throw new Error(
        'loadViewer expects a div with id `forgeViewer` to exist in the document'
      )
    }
    const viewer = new Autodesk.Viewing.Viewer3D(htmlDiv)

    const dbIdGuidDicts: DbGuidDicts = {}
    const dbIdPropertyGuidDicts: DbPropertyGuidDicts = {}
    const urnIndexModelIdDict: UrnIndexModelIdDict = {}

    const onDocumentLoadSuccess =
      (index) => async (viewerDocument: Autodesk.Viewing.Document) => {
        const defaultModel = viewerDocument.getRoot().getDefaultGeometry()
        const model = await viewer.loadDocumentNode(
          viewerDocument,
          defaultModel,
          {
            keepCurrentModels: true,
            acmSessionId: viewerDocument.acmSessionId,
            globalOffset: viewer.model?.getData().globalOffset,
          }
        )
        onModelLoadChange(index, 'success')
        urnIndexModelIdDict[index] = model.id
      }

    const onDocumentLoadFailure = (index) => () => {
      onModelLoadChange(index, 'failure')
      logger.error('Failed fetching Forge manifest')
    }

    viewer.addEventListener(Autodesk.Viewing.VIEWER_INITIALIZED, () => {
      initialize(viewer as Autodesk.Viewing.Viewer3D & ClickHandler)
    })

    viewer.addEventListener(Autodesk.Viewing.MODEL_ROOT_LOADED_EVENT, () =>
      initialize(viewer as Autodesk.Viewing.Viewer3D & ClickHandler)
    )

    let modelLoadingCount = 0
    viewer.addEventListener(
      Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
      async ({ model }) => {
        const modelId = model.getModelId()
        try {
          const dbIdPropertiesDict = await getDbIdPropertiesDict(
            model.getPropertyDb(),
            logger
          )
          dbIdPropertyGuidDicts[modelId] =
            getDbIdPropertyGuidDict(dbIdPropertiesDict)
        } catch {
          dbIdPropertyGuidDicts[modelId] = {}
        }

        model.getExternalIdMapping(
          async (data) => {
            const dbIds: number[] = Object.values(data)
            const guids = await getGuids(model, dbIds)
            dbIdGuidDicts[modelId] = getDbIdGuidDict(dbIds, guids)

            model.unconsolidate()
            const [urnIndex] =
              Object.entries(urnIndexModelIdDict).find(
                ([_, mId]) => modelId === mId
              ) ?? []
            if (urnIndex != null && layers[urnIndex]) {
              updateLayers(
                viewer,
                model,
                layers[urnIndex],
                dbIdGuidDicts[modelId],
                getColor
              )
            }
          },
          (e) => {
            logger.error(e)
          }
        )
        modelLoadingCount += 1
        if (modelLoadingCount === defaultDisplayUrnIndices.length) {
          finalize({
            getControls: getViewerControls,
            viewer,
            urnIndexModelIdDict,
            dbIdGuidDicts,
            dbIdPropertyGuidDicts,
            urns,
            onDocumentLoadSuccess,
            onDocumentLoadFailure,
            onModelLoadChange,
            getColor,
            viewerCb,
          })
        }
      }
    )

    if (defaultDisplayUrnIndices.length === 0) {
      finalize({
        getControls: getViewerControls,
        viewer,
        urnIndexModelIdDict,
        dbIdGuidDicts,
        dbIdPropertyGuidDicts,
        urns,
        onDocumentLoadSuccess,
        onDocumentLoadFailure,
        onModelLoadChange,
        getColor,
        viewerCb,
      })
    }

    viewer.addEventListener(
      Autodesk.Viewing.AGGREGATE_SELECTION_CHANGED_EVENT,
      aggregateSelectionChangedListener(dbIdGuidDicts, onClick)
    )

    let isMouseOverOn = false
    viewer.addEventListener(
      Autodesk.Viewing.OBJECT_UNDER_MOUSE_CHANGED,
      objectUnderMouseChangedListener(
        { dbIdGuidDicts, urnIndexModelIdDict },
        onMouseHoverChange,
        (mouseOverState: boolean) => {
          isMouseOverOn = mouseOverState
        }
      )
    )

    const POSITION_PADDING = { x: -16, y: 38 }
    viewer?.canvas?.addEventListener('mousemove', (e) =>
      mouseMoveListener(
        isMouseOverOn,
        onMousePositionChange,
        POSITION_PADDING
      )(e)
    )

    viewer.start()

    defaultDisplayUrnIndices.forEach((urnIndex) => {
      loadModel(Autodesk.Viewing.Document.load)(urnIndex, urns, {
        onDocumentLoadSuccess,
        onDocumentLoadFailure,
      })
    })
  }

  Autodesk.Viewing.Initializer(
    {
      getAccessToken(onSuccess) {
        onSuccess?.(accessToken, 30000)
      },
      language: 'en',
    },
    viewingInitializer
  )
}

interface ViewerProps {
  urns: string[]
  token: string
  layers?: LayerInfo[]
  defaultDisplayUrnIndices?: number[]
  onClick?: (e: any) => void
  onInit?: (controls: ViewerControls) => void
  onModelLoad?: (urnIndex: UrnIndex, status: ModelStatus) => void
  getColor?: (priority: number) => THREE.Vector4
  colorMap?: { [key: string]: string }
  className?: string
  children?: ReactNode
}

export default function Viewer3D({
  urns = [],
  token,
  layers = [],
  defaultDisplayUrnIndices = [],
  onClick = () => {},
  onInit,
  onModelLoad = () => {},
  getColor = getDefaultColor,
  colorMap = defaultPriorityColors,
  className,
  children,
}: ViewerProps) {
  const [autodeskViewer, setAutodeskViewer] =
    useState<Autodesk.Viewing.Viewer3D>()
  const [innerViewerControls, setInnerViewerControls] = useState<any>()
  const [mousePosition, setMousePosition] = useState<MousePosition>()
  const [hoveredGuid, setHoveredGuid] = useState<string>()
  const [selectedModelIndex, setSelectedModelIndex] = useState<number>()
  const logger = useConsole()

  useEffect(() => {
    loadScript(() => {
      loadViewer({
        accessToken: token,
        urns,
        layers,
        defaultDisplayUrnIndices,
        viewerCb: (
          viewer,
          viewerControls: ViewerControls,
          innerViewerCtrls
        ) => {
          setAutodeskViewer(viewer)
          setInnerViewerControls(innerViewerCtrls)
          if (onInit) {
            onInit(viewerControls)
          }
        },
        onMousePositionChange: (position: MousePosition) => {
          setMousePosition(position)
        },
        onMouseHoverChange: (guid: string, modelIndex: number) => {
          setHoveredGuid(guid)
          setSelectedModelIndex(modelIndex)
        },
        onClick,
        onModelLoadChange: (urnIndex: UrnIndex, status: ModelStatus) => {
          onModelLoad(urnIndex, status)
        },
        getColor,
        logger,
      })
    })
  }, [])

  useEffect(() => {
    const $tooltipTriggerEl = document.getElementById('model-tooltip-trigger')

    if (autodeskViewer && hoveredGuid && $tooltipTriggerEl) {
      const layerName =
        selectedModelIndex != null
          ? getLayerName(layers, selectedModelIndex, hoveredGuid)
          : undefined

      updateTooltipStyles($tooltipTriggerEl, {
        mousePosition,
        display:
          selectedModelIndex != null && selectedModelIndex > -1 && !!layerName,
      })
      updateViewerCursor(autodeskViewer, mousePosition)
    }
  }, [autodeskViewer, mousePosition, hoveredGuid, selectedModelIndex, layers])

  useEffect(() => {
    if (layers.length > 0 && innerViewerControls) {
      layers.forEach((layer, i) => {
        innerViewerControls.updateLayersByUrnIndex(i, layer)
      })
    }
  }, [layers, innerViewerControls])

  return (
    <ResizeObserverContainer
      onContainerWidthChange={() => {
        autodeskViewer?.resize()
      }}
      id="forgeViewer"
      role="application"
      className={className}
    >
      {hoveredGuid && (
        <Tooltip
          label={
            selectedModelIndex != null
              ? getLayerName(layers, selectedModelIndex, hoveredGuid)
              : ''
          }
          opened
          position="bottom-start"
        >
          <Box id="model-tooltip-trigger" pos="absolute" />
        </Tooltip>
      )}
      {children}
    </ResizeObserverContainer>
  )
}

/**
 * Set up controls and expose them
 * to external user by running a callback function
 * at the end; without calling the function, user
 * will not be able to turn on/off (show/hide) a model.
 *
 * note urns refer to the "urn" of each model which
 * needs to be provided to auto desk viewer while
 * urnIndexModelIdDict maps index of the urn to
 * model id.
 */
const finalize = ({
  getControls,
  viewer,
  urnIndexModelIdDict,
  dbIdGuidDicts,
  dbIdPropertyGuidDicts,
  urns,
  onDocumentLoadSuccess,
  onDocumentLoadFailure,
  onModelLoadChange,
  getColor,
  viewerCb,
}: {
  getControls: typeof getViewerControls
  viewer: Autodesk.Viewing.Viewer3D
  urnIndexModelIdDict: UrnIndexModelIdDict
  dbIdGuidDicts: DbGuidDicts
  dbIdPropertyGuidDicts: DbPropertyGuidDicts
  urns: string[]
  onDocumentLoadSuccess: (
    index
  ) => (viewerDocument: Autodesk.Viewing.Document) => void
  onDocumentLoadFailure: (index) => () => void
  onModelLoadChange: (urnIndex: UrnIndex, status: ModelStatus) => void
  getColor: (priority: number) => THREE.Vector4
  viewerCb: ViewerCb
}) => {
  const viewerControls = getControls(
    viewer,
    {
      urnIndexModelIdDict,
      dbIdGuidDicts,
      dbIdPropertyGuidDicts,
    },
    loadModel(Autodesk.Viewing.Document.load),
    urns,
    {
      onDocumentLoadSuccess,
      onDocumentLoadFailure,
    },
    onModelLoadChange
  )
  const innerViewerControls = getInnerViewerControls(
    viewer,
    {
      urnIndexModelIdDict,
      dbIdGuidDicts,
    },
    getColor
  )
  viewerCb(viewer, viewerControls, innerViewerControls)
}

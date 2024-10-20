/* eslint-disable react-hooks/exhaustive-deps, complexity */
import { useState, useRef, useEffect, forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import {
  assetModelId,
  buildingModelId,
  levelModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import {
  ModelStatus,
  ViewerControls,
} from '@willow/ui/components/Viewer3D/types'
import { Viewer3D, IconNew } from '@willow/ui'
import { Badge, Tabs, TabAndPanel } from '@willowinc/ui'

import useTwinAnalytics from '../../useTwinAnalytics'
import { useAutoDeskToken } from '../../../../../hooks/index'
import { Dropdown, DropdownContent } from './Dropdown/Dropdown'
import {
  constructDefaultIndices,
  constructRenderDropdownObject,
  getEnabledLayersCount,
  toggleModel,
} from './Dropdown/utils'
import { ThreeDModelTabProps } from './types'
import { RenderDropdownObject, RenderLayer } from './Dropdown/types'
import { useTwinBuilding, useTwinLevel, useTwinAsset } from './hooks'
import useOntology from '../../../../../hooks/useOntologyInPlatform'

export default function useThreeDModelTab({
  siteID,
  modelInfo,
  geometrySpatialReference, // TODO: will address whether to remove/keep with https://dev.azure.com/willowdev/Unified/_workitems/edit/61584
  asset,
  twin,
}: ThreeDModelTabProps): TabAndPanel | undefined {
  const analytics = useTwinAnalytics()
  const ontologyQuery = useOntology()
  const { t } = useTranslation()

  const tabHeaderRef = useRef<HTMLDivElement>(null)

  // Control displaying dropdown
  const [isShown, setShown] = useState(false)
  const [modelLoadingState, setModelLoadingState] =
    useState<ModelStatus>('idle')

  /**
   *  Cases for different type of twin (building, level, and asset)
   *  1. building:
   *    a. when building only have 3d building model loaded,
   *        - show 3d model tab
   *        - turn on 3d building model by default
   *        - the model will be called "Site" in the dropdown menu
   *    b. when building only have "bldg"/"Site" floor model loaded,
   *        - show 3d model tab
   *        - 3d dropdown list will show the 3D model groups, they will be named, grouped and sorted according to the names, disciplines and groups for the floors as configured in admin
   *        - turn on the default 3D models for the "Site" floor by default
   *    c. when building has 3d building model loaded and "bldg"/"Site" floor models loaded,
   *        - show 3d model tab
   *        - turn on the 3d building model by default
   *        - 3d dropdown list of models will show the "Site" model first, followed by the other "Site" 3D model groups and disciplines in the configured order in admin
   *    d. when no 3d building model is loaded and no "Bldg" / "Site" floor's model is loaded
   *        - do not show 3d model tab
   *  2. level:
   *    a. when level twin for that building has 3D models loaded,
   *        - show 3d model tab
   *        - show models in dropdown
   *        - turn on the default 3D model(s) for the level twin by default
   *  3. asset:
   *
   */

  const [viewerControls, setViewerControls] = useState<
    ViewerControls | undefined
  >()

  const [renderDropdownObject, setRenderDropdownObject] =
    useState<RenderDropdownObject>({})

  // 3d model should only be displayed for building, level, asset twins
  const ancestors = ontologyQuery.data?.getModelAncestors(
    modelInfo?.model['@id']
  )
  const isBuildingTwin = ancestors?.includes(buildingModelId)
  const isLevelTwin = !!ancestors?.includes(levelModelId)
  const isAssetTwin = ancestors?.includes(assetModelId)

  const shouldAcquireAutodeskToken =
    isBuildingTwin || isLevelTwin || isAssetTwin

  // Get autodesk viewer's access token
  const { data: autoDeskData, status: autoDeskTokenStatus } = useAutoDeskToken({
    enabled: shouldAcquireAutodeskToken,
  })

  const {
    data: {
      building3dModels,
      is3dTabForBuildingEnabled,
      building3dModelsIds,
      buildingDefaultUrns,
    },
  } = useTwinBuilding({
    siteId: siteID,
    options: {
      enabled:
        isBuildingTwin &&
        typeof siteID === 'string' &&
        autoDeskTokenStatus === 'success',
    },
    autoDeskData,
  })

  const {
    data: {
      assetModels,
      assetModelIds,
      defaultAssetModule,
      assetDefaultUrns,
      is3dTabForAssetEnabled,
    },
  } = useTwinAsset({
    siteId: siteID || '',
    floorId: asset?.floorId,
    moduleTypeNamePath: asset?.moduleTypeNamePath,
    options: {
      enabled: isAssetTwin && typeof asset?.floorId === 'string' && !!siteID,
    },
  })

  const {
    data: {
      levelModels,
      levelModelIds,
      levelDefaultUrns,
      is3dTabForLevelEnabled,
    },
  } = useTwinLevel({
    siteId: siteID || '',
    floorId: asset?.id,
    dependencies: isLevelTwin && !!siteID && !!asset?.id,
  })

  useEffect(() => {
    if (isBuildingTwin) {
      setRenderDropdownObject(constructRenderDropdownObject(building3dModels))
    }

    if (isAssetTwin) {
      setRenderDropdownObject(
        constructRenderDropdownObject(assetModels, asset?.moduleTypeNamePath)
      )
    }

    if (isLevelTwin) {
      setRenderDropdownObject(constructRenderDropdownObject(levelModels))
    }
  }, [
    asset?.id,
    building3dModelsIds,
    asset?.moduleTypeNamePath,
    assetModelIds,
    levelModelIds,
  ])

  const [defaultUrnIndices, setDefaultUrnIndices] = useState<number[]>([])
  const shouldSetDefaultIndices = useRef(true)

  let defaultUrns = [] as string[]

  switch (true) {
    case isAssetTwin:
      defaultUrns = assetDefaultUrns
      break
    case isBuildingTwin:
      defaultUrns = buildingDefaultUrns
      break
    case isLevelTwin:
      defaultUrns = levelDefaultUrns
      break
    default:
      defaultUrns = []
  }

  useEffect(() => {
    analytics.trackThreeDimensionViewed(twin)
  }, [analytics, twin.id])

  useEffect(() => {
    if (
      Object.keys(renderDropdownObject).length > 0 &&
      shouldSetDefaultIndices.current
    ) {
      setDefaultUrnIndices(
        constructDefaultIndices({
          renderDropdownObject,
          urns: defaultUrns,
        })
      )
      shouldSetDefaultIndices.current = false
    }
  }, [
    renderDropdownObject,
    buildingDefaultUrns,
    assetDefaultUrns,
    levelDefaultUrns,
  ])

  useEffect(() => {
    // asset twin logic to highlight and zoom in on the asset
    if (
      isAssetTwin &&
      viewerControls &&
      defaultAssetModule?.url &&
      asset?.forgeViewerModelId &&
      assetDefaultUrns.indexOf(defaultAssetModule?.url) !== -1
    ) {
      viewerControls.select({
        type: 'asset',
        urnIndex: assetDefaultUrns.indexOf(defaultAssetModule?.url),
        guid: asset?.forgeViewerModelId,
      })
    }
  }, [
    viewerControls,
    asset?.forgeViewerModelId,
    defaultAssetModule?.url,
    isAssetTwin,
  ])

  useEffect(() => {
    try {
      toggleModel({
        urns: defaultUrns,
        modelUrn: dropdownLayerRef.current?.url,
        viewerControls,
        isEnable: dropdownLayerRef.current?.isEnabled,
      })
    } catch (error) {
      console.error(error)
    }
  }, [renderDropdownObject, viewerControls])

  const dropdownLayerRef = useRef<Partial<RenderLayer>>()

  // Toggle layer's isEnabled
  const toggleDropdownLayer = (
    sectionName: string,
    layerName: string,
    isUngroupedLayer: boolean
  ) => {
    const newRenderDropdownObject = { ...renderDropdownObject }

    if (isUngroupedLayer) {
      newRenderDropdownObject[sectionName].isEnabled =
        !renderDropdownObject[sectionName].isEnabled
      dropdownLayerRef.current = newRenderDropdownObject[sectionName]
    } else {
      newRenderDropdownObject[sectionName][layerName].isEnabled =
        !renderDropdownObject[sectionName][layerName].isEnabled
      dropdownLayerRef.current = newRenderDropdownObject[sectionName][layerName]
    }

    setRenderDropdownObject(newRenderDropdownObject)
  }

  const is3dModelTabEnabled =
    shouldSetDefaultIndices.current === false &&
    ((isBuildingTwin && is3dTabForBuildingEnabled) ||
      (isAssetTwin && is3dTabForAssetEnabled) ||
      (isLevelTwin && is3dTabForLevelEnabled))

  if (!siteID || !is3dModelTabEnabled || autoDeskTokenStatus !== 'success')
    return undefined

  const handleInitControls = (control: ViewerControls) =>
    setViewerControls(control)

  return [
    <Tabs.Tab
      data-testid="twin-threeDModel-tab"
      suffix={<Badge>{getEnabledLayersCount(renderDropdownObject)}</Badge>}
      value="threeDModel"
    >
      <TabHeader
        isExpanded={isShown}
        label={t('twinExplorer.3dModel')}
        onClick={() => setShown(!isShown)}
        ref={tabHeaderRef}
      />
    </Tabs.Tab>,
    <Tabs.Panel value="threeDModel">
      <Dropdown
        tabHeaderRef={tabHeaderRef}
        isShown={isShown}
        setShown={setShown}
        dropdownContent={
          <DropdownContent
            renderDropdownObject={renderDropdownObject}
            toggleDropdownLayer={toggleDropdownLayer}
            $isLoading={modelLoadingState === 'loading'}
          />
        }
      />

      <Viewer3DContainer>
        <Viewer3DWrapper
          urns={defaultUrns}
          defaultDisplayUrnIndices={defaultUrnIndices}
          token={autoDeskData.access_token}
          onInit={handleInitControls}
          onModelLoad={(_index: number, status: ModelStatus) => {
            setModelLoadingState(status)
          }}
        />
      </Viewer3DContainer>
    </Tabs.Panel>,
  ]
}

const Viewer3DContainer = styled.div(({ theme }) => ({
  height: '100%',
  width: '100%',
  padding: theme.spacing.s4,
}))

const Viewer3DWrapper = styled(Viewer3D)({
  width: '100%',
  height: '100%',
})

export const TabHeader = forwardRef<
  HTMLDivElement,
  { onClick: () => void; isExpanded: boolean; label: string }
>(({ isExpanded, onClick, label }, ref) => (
  <div ref={ref} tw="flex align-items[center]">
    {label}

    <IconNew
      tw="ml-1 hover:color[#fafafa]"
      onClick={onClick}
      icon={isExpanded ? 'layersCollapse' : 'layersExpand'}
      size="small"
    />
  </div>
))

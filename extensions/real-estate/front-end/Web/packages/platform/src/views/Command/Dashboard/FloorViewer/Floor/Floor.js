/* eslint-disable complexity */
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { WALMART_ALERT } from '@willow/common/insights/insights/types'
import { NotFound, useTimer, useWindowEventListener } from '@willow/ui'
import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import _ from 'lodash'
import { useSite } from 'providers'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css } from 'styled-components'
import 'twin.macro'
import { useGetSelectedAsset } from '../../../../../services/Assets/useGetSelectedAsset'
import { TicketSourceType } from '../../../../../services/Tickets/TicketsService'
import { useDashboard } from '../../DashboardContext'
import AssetSelector from './AssetSelector/AssetSelector'
import AssetDetailPanel from './Editor/AssetDetailPanel/AssetDetailPanel'
import Editor from './Editor/Editor'
import EditorGIS from './Editor/EditorGIS/EditorGIS'
import ErrorModal from './ErrorModal'
import { FloorContext } from './FloorContext'
import SidePanel from './SidePanel/SidePanel'
import useSaveFloor from './useSaveFloor'
import useStatistics from './useStatistics'

export default function Floor({
  floor,
  equipment: equipmentProp,
  disciplinesSortOrder,
  isFloorEditorDisabled = false,
}) {
  const [selectedDataTab, setSelectedDataTab] = useState('insights')
  const [viewerLoaded, setViewerLoaded] = useState(false)
  const { t } = useTranslation()
  const { isReadOnly } = useDashboard()
  const site = useSite()
  const timer = useTimer()
  const [
    {
      assetId,
      isInsightStatsLayerOn,
      isTicketStatsLayerOn,
      floorViewType = '3D',
    },
    setSearchParams,
  ] = useMultipleSearchParams([
    'assetId',
    'isInsightStatsLayerOn',
    'isTicketStatsLayerOn',
    'floorViewType',
  ])
  // Local state to be used in filtering insight statistics based on source;
  // this is only relevant for Walmart as they have their own Insights.
  // All Willow generated Insights are said to be from 'Willow Activate' as
  // that is a brand name for Willow's Insights, and Walmart's Insights
  // all have rule id called "walmart_alert".
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/131984
  const defaultLayerSources = ['activate', WALMART_ALERT]
  const [insightLayerSources, setInsightLayerSources] = useState(
    isInsightStatsLayerOn ? defaultLayerSources : []
  )
  // TODO: The setup of lumping other ticket sources into a single checkbox will be changed
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/132340
  const defaultTicketLayerSources = [
    TicketSourceType.willow,
    TicketSourceType.mapped,
    // We cannot associated an array of TicketSourceType with checkbox's input value,
    // so we need to join them into a string for the values we want to put under a
    // a single checkbox
    [TicketSourceType.app, TicketSourceType.dynamics].join(','),
  ]
  const [ticketLayerSources, setTicketLayerSources] = useState(
    isTicketStatsLayerOn ? defaultTicketLayerSources : []
  )

  // Accept message from 3D Viewer iframe to change selectedDataTab
  useWindowEventListener('message', (event) => {
    if (event.data.type === 'selectedDataTabChange') {
      setSelectedDataTab(event.data.selectedDataTab)
    }
  })

  const [state, setState] = useState(() => {
    const initialFloor = {
      ...floor,
      layerGroups: [
        ..._(floor.layerGroups)
          .orderBy((layer) => layer.name !== 'Assets layer')
          .thru((layers) =>
            !layers.some((layer) => layer.name === 'Assets layer')
              ? [
                  {
                    name: 'Assets layer',
                    is3D: false,
                    equipments: [],
                    layers: [],
                    zones: [],
                  },
                  ...layers,
                ]
              : layers
          )
          .map((layerGroup) => ({
            ...layerGroup,
            localId: _.uniqueId(),
            zones: layerGroup.zones.map((zone) => ({
              id: zone.id,
              localId: _.uniqueId(),
              points: zone.geometry,
              equipmentIds: zone.equipmentIds,
            })),
            equipments: layerGroup.equipments.map((equipment) => ({
              id: equipment.id,
              localId: _.uniqueId(),
              name: equipment.name,
              hasInsights: equipment.hasInsights,
              hasLiveData: equipment.hasLiveData,
              priority: equipment.priority,
              points: equipment.geometry,
              pointTags: equipment.pointTags
                .filter((pointTag) => pointTag.feature === '2d')
                .map((pointTag) => ({
                  ...pointTag,
                  layer: layerGroup.layers.find(
                    (layer) => layer.tagName === pointTag.name
                  ),
                })),
            })),
            layers: layerGroup.layers.map((layer) => ({
              ...layer,
              localId: _.uniqueId(),
            })),
          }))
          .value(),
        {
          id: 'floor_layer',
          localId: _.uniqueId(),
          name: 'Floor layer',
          hasLoaded: false,
          zones: [],
          equipments: [],
          layers: [],
        },
      ],
      modules2D: _(floor.modules2D)
        .orderBy((image) => image.sortOrder)
        .map((image) => ({
          ...image,
          isVisible: image.isDefault,
        }))
        .value(),
    }

    const visibleLayerGroupIds = initialFloor.layerGroups
      .filter(
        (layerGroup) =>
          !isReadOnly ||
          (layerGroup.name !== 'Assets layer' &&
            layerGroup.name !== 'Floor layer')
      )
      .filter((layerGroup) =>
        layerGroup.equipments.some((equipment) => equipment.priority > 0)
      )
      .map((layerGroup) => [layerGroup.id, layerGroup.name])

    return {
      floor: initialFloor,
      lastSavedTime: undefined,
      mode: undefined,
      selectedLayerGroupLocalId: undefined,
      centeredEquipmentId: undefined,
      selectedAsset:
        equipmentProp != null
          ? {
              id: equipmentProp.id,
              equipmentId: equipmentProp.equipmentId,
              hasLiveData: equipmentProp.hasLiveData,
              isEquipmentOnly: true,
              moduleTypeNamePath: convertModuleTypeNamePath(
                equipmentProp.moduleTypeNamePath
              ),
              tags: [],
              pointTags: [],
            }
          : undefined,
      equipment: equipmentProp,
      visibleLayerGroupIds,
    }
  })
  const [showErrorModal, setShowErrorModal] = useState(false)
  const [selectedLayerIds, setSelectedLayerIds] = useState([])

  const iframeRef = useRef()

  const statsQuery = useStatistics(
    {
      floorId: floor.floorId,
      siteId: site.id,
      moduleTypeNamePaths:
        floor?.modules3D
          ?.filter((module) => selectedLayerIds?.includes(module.id))
          ?.map((module) => module.typeName) || [],
      includeRuleId: _.isEqual(insightLayerSources, [WALMART_ALERT])
        ? WALMART_ALERT
        : undefined,
      excludeRuleId: _.isEqual(insightLayerSources, ['activate'])
        ? WALMART_ALERT
        : undefined,
      // The underlying values of ticketLayerSources is ['a', 'b', 'c,d'], so we join them
      // by ',' and then split them by ',' to get the array of values
      ticketSourceTypes: ticketLayerSources.length
        ? ticketLayerSources.join(',').split(',')
        : [],
    },
    {
      // keep old data since new data will be a
      // superset of old data
      keepPreviousData: true,
    }
  )

  useSaveFloor({
    floor: state.floor,

    updateLayerGroup(lastSavedFloor, layerGroup, nextLayerGroup) {
      function updateFloor(nextFloor) {
        return {
          ...nextFloor,
          layerGroups: nextFloor.layerGroups.map((prevLayerGroup) =>
            prevLayerGroup.localId === layerGroup.localId
              ? {
                  ...prevLayerGroup,
                  id: nextLayerGroup.id,
                  zones: prevLayerGroup.zones.map((prevZone, i) => ({
                    ...prevZone,
                    id: nextLayerGroup.zones[i]?.id,
                  })),
                }
              : prevLayerGroup
          ),
        }
      }

      setState((prevState) => {
        const nextFloor = updateFloor(prevState.floor)

        return {
          ...prevState,
          floor: nextFloor,
        }
      })

      return updateFloor(lastSavedFloor)
    },

    onSaving() {
      setState((prevState) => ({
        ...prevState,
        lastSavedTime: 'loading',
      }))
    },

    onSaved() {
      setState((prevState) => ({
        ...prevState,
        lastSavedTime: new Date().toISOString(),
      }))
    },

    onError() {
      setShowErrorModal(true)
    },
  })

  const nextLayerGroup = state.floor.layerGroups.find(
    (layerGroup) => layerGroup.localId === state.selectedLayerGroupLocalId
  )
  let nextVisibleLayerGroups = state.floor.layerGroups
    .filter(
      (layerGroup) =>
        layerGroup.name !== 'Assets layer' && layerGroup.name !== 'Floor layer'
    )
    .filter((layerGroup) => state.visibleLayerGroupIds.includes(layerGroup.id))

  if (!isReadOnly) {
    nextVisibleLayerGroups = []
    if (nextLayerGroup != null) {
      if (nextLayerGroup.name === 'Assets layer') {
        nextVisibleLayerGroups = state.floor.layerGroups
      } else {
        nextVisibleLayerGroups = [nextLayerGroup]
      }
    }
  }

  function splitModuleTypeNameSection(section) {
    return section.includes('|')
      ? section.split('|')
      : section.includes(',')
      ? section.split(',')
      : [section]
  }

  function convertModuleTypeNamePath(moduleTypeNamePath) {
    if (!moduleTypeNamePath) return moduleTypeNamePath
    return Array.isArray(moduleTypeNamePath)
      ? _.flatten(
          moduleTypeNamePath.map((currentModule) =>
            splitModuleTypeNameSection(currentModule)
          )
        )
      : splitModuleTypeNameSection(moduleTypeNamePath)
  }

  function getModuleForModuleTypeName(modules, moduleTypeNamePath) {
    if (!moduleTypeNamePath) return null

    const moduleTypeNamePathSplit =
      convertModuleTypeNamePath(moduleTypeNamePath)

    let layer = null
    for (let i = 0; i < moduleTypeNamePathSplit.length; i++) {
      const currentModuleType = moduleTypeNamePathSplit[i].toLowerCase()
      const floorModule = modules.find(
        (mod) => currentModuleType === mod.typeName.toLowerCase()
      )
      if (floorModule) {
        layer = floorModule
        break
      }
    }

    return layer
  }

  const context = {
    ...state.floor,
    isFloorEditorDisabled,
    lastSavedTime: state.lastSavedTime,
    mode: state.mode ?? 'view',
    layerGroup: nextLayerGroup,
    visibleLayerGroups: nextVisibleLayerGroups,
    selectedEquipmentId: state.selectedAsset?.isEquipmentOnly
      ? state.selectedAsset.id
      : undefined,
    selectedAsset: state.selectedAsset,
    hasChanged: false,
    centeredEquipmentId: state.centeredEquipmentId,
    equipment: state.equipment,
    selectedLayerIds,
    setSelectedLayerIds,
    isReadOnly,
    floorViewType,
    iframeRef,
    state,
    disciplinesSortOrder,
    statsQuery,
    isInsightStatsLayerOn,
    isTicketStatsLayerOn,
    viewerLoaded,
    setViewerLoaded,
    insightLayerSources,
    ticketLayerSources,

    onInsightLayerSourcesChange(nextSources) {
      setInsightLayerSources(nextSources)
    },

    onTicketLayerSourcesChange(nextSources) {
      setTicketLayerSources(nextSources)
    },

    onResetInsightLayerSources() {
      setInsightLayerSources(defaultLayerSources)
    },

    onResetTicketLayerSources() {
      setTicketLayerSources(defaultTicketLayerSources)
    },

    onInsightStatsLayerOpenChange(nextStatus) {
      setSearchParams({
        isInsightStatsLayerOn: nextStatus ? 1 : undefined,
      })
    },

    onTicketStatsLayerOpenChange(nextStatus) {
      setSearchParams({
        isTicketStatsLayerOn: nextStatus ? 1 : undefined,
      })
    },

    floorLayer: state.floor.layerGroups.find(
      (layerGroup) => layerGroup.id === 'floor_layer'
    ),

    isLayerGroupVisible(layerGroup) {
      return state.visibleLayerGroupIds.includes(layerGroup.id)
    },

    selectImage(imageId) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          modules2D: prevState.floor.modules2D.map((image) =>
            image.id === imageId
              ? {
                  ...image,
                  isVisible: true,
                }
              : image
          ),
        },
      }))
    },

    toggleImage(imageId) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          modules2D: prevState.floor.modules2D.map((image) =>
            image.id === imageId
              ? {
                  ...image,
                  isVisible: !image.isVisible,
                }
              : image
          ),
        },
      }))
    },

    removeImage(imageId) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          modules2D: prevState.floor.modules2D.filter(
            (image) => image.id !== imageId
          ),
        },
      }))
    },

    removeModel(layerId) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          modules3D: prevState.floor.modules3D.filter(
            (layer) => layer.id !== layerId
          ),
        },
      }))
    },

    selectMode(mode) {
      setState((prevState) => ({
        ...prevState,
        mode: prevState.mode !== mode ? mode : undefined,
      }))
    },

    selectLayerGroup(layerGroup) {
      setState((prevState) => ({
        ...prevState,
        mode: layerGroup?.name === 'Assets layer' ? 'view' : prevState.mode,
        selectedLayerGroupLocalId:
          prevState.selectedLayerGroupLocalId !== layerGroup.localId
            ? layerGroup.localId
            : undefined,
      }))
    },

    selectAsset(asset) {
      setState((prevState) => ({
        ...prevState,
        selectedAsset:
          prevState.selectedAsset?.id !== asset?.id ? asset : undefined,
      }))
      handleAssetIdChange(asset?.id)
    },

    addLayerGroup() {
      setState((prevState) => {
        const localId = _.uniqueId()

        return {
          ...prevState,
          floor: {
            ...prevState.floor,
            layerGroups: [
              ...prevState.floor.layerGroups,
              {
                localId,
                name: 'New layer',
                zones: [],
                equipments: [],
              },
            ],
          },
          selectedLayerGroupLocalId: localId,
        }
      })
    },

    deleteLayerGroup(layerGroup) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups
            .filter((prevLayerGroup) => prevLayerGroup !== layerGroup)
            .map((prevLayerGroup) =>
              prevLayerGroup.name === 'Assets layer'
                ? {
                    ...prevLayerGroup,
                    equipments: [
                      ...prevLayerGroup.equipments,
                      ...layerGroup.equipments,
                    ],
                  }
                : prevLayerGroup
            ),
        },
        selectedLayerGroupLocalId: undefined,
      }))
    },

    setFloorImageName(imageId, name) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          modules2D: prevState.floor.modules2D.map((image) =>
            image.id === imageId
              ? {
                  ...image,
                  name,
                }
              : image
          ),
        },
      }))
    },

    setLayerGroupName(layerGroup, name) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups.map((prevLayerGroup) =>
            prevLayerGroup.localId === layerGroup.localId &&
            name !== 'Assets layer' &&
            name !== 'Floor layer'
              ? {
                  ...prevLayerGroup,
                  name,
                }
              : prevLayerGroup
          ),
        },
      }))
    },

    addZone(zone) {
      if (state.selectedLayerGroupLocalId == null) {
        context.addLayerGroup()
      }

      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups.map((layerGroup) =>
            layerGroup.localId === prevState.selectedLayerGroupLocalId
              ? {
                  ...layerGroup,
                  zones: [
                    ...layerGroup.zones,
                    {
                      ..._.omit(zone, 'id'),
                      localId: _.uniqueId(),
                    },
                  ],
                }
              : layerGroup
          ),
        },
      }))
    },

    addEquipment(equipment) {
      setState((prevState) => {
        const selectedLocalId =
          prevState.selectedLayerGroupLocalId ??
          prevState.floor.layerGroups.find(
            (layerGroup) => layerGroup.name === 'Assets layer'
          )?.localId

        return {
          ...prevState,
          selectedLayerGroupLocalId: selectedLocalId,
          floor: {
            ...prevState.floor,
            layerGroups: prevState.floor.layerGroups.map((layerGroup) =>
              layerGroup.localId === selectedLocalId
                ? {
                    ...layerGroup,
                    equipments: [
                      ...layerGroup.equipments,
                      {
                        ...equipment,
                        localId: _.uniqueId(),
                      },
                    ],
                  }
                : layerGroup
            ),
          },
        }
      })
    },

    updateObject(nextObject) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups.map((layerGroup) => ({
            ...layerGroup,
            zones: layerGroup.zones.map((zone) =>
              zone.localId === nextObject.localId ? nextObject : zone
            ),
            equipments: layerGroup.equipments.map((equipment) =>
              equipment.localId === nextObject.localId ? nextObject : equipment
            ),
          })),
        },
      }))
    },

    deleteObject(nextObject) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups.map((layerGroup) => ({
            ...layerGroup,
            zones: layerGroup.zones.filter(
              (zone) => zone.localId !== nextObject.localId
            ),
            equipments: layerGroup.equipments.filter(
              (equipment) => equipment.localId !== nextObject.localId
            ),
          })),
        },
      }))
    },

    updateName(floorName) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          floorName,
        },
      }))
    },

    setFloorLayerGeometry(geometry) {
      setState((prevState) => ({
        ...prevState,
        floor: {
          ...prevState.floor,
          layerGroups: prevState.floor.layerGroups.map((layerGroup) =>
            layerGroup.id === 'floor_layer'
              ? {
                  ...layerGroup,
                  hasLoaded: true,
                  zones: geometry.map((points) => ({
                    localId: _.uniqueId(),
                    points,
                    equipmentIds: [],
                  })),
                }
              : layerGroup
          ),
        },
      }))
    },

    toggleIsVisible(layerGroup) {
      setState((prevState) => ({
        ...prevState,
        visibleLayerGroupIds: _.xor(prevState.visibleLayerGroupIds, [
          layerGroup.id,
        ]),
      }))
    },

    hideLayerGroups() {
      setState((prevState) => ({
        ...prevState,
        visibleLayerGroupIds: [],
      }))
    },

    setLayerGroupId(equipment, layerGroupId) {
      setState((prevState) => {
        const nextLayerGroupId =
          layerGroupId ??
          prevState.floor.layerGroups.find(
            (layerGroup) => layerGroup.name === 'Assets layer'
          )?.id

        return {
          ...prevState,
          floor: {
            ...prevState.floor,
            layerGroups: prevState.floor.layerGroups.map((layerGroup) => {
              const equipments = layerGroup.equipments.filter(
                (layerGroupEquipment) => layerGroupEquipment.id !== equipment.id
              )

              return {
                ...layerGroup,
                equipments:
                  layerGroup.id === nextLayerGroupId
                    ? [...equipments, equipment]
                    : equipments,
              }
            }),
          },
        }
      })
    },

    async showSelectedAsset() {
      setState((prevState) => {
        const selectedLayerGroupId = prevState.floor.layerGroups.find(
          (layerGroup) =>
            layerGroup.equipments.some(
              (equipment) =>
                equipment.id === prevState.selectedAsset?.id ||
                equipment.id === prevState.selectedAsset?.equipmentId
            )
        )?.id

        return {
          ...prevState,
          visibleLayerGroupIds: [
            ...new Set([
              ...prevState.visibleLayerGroupIds,
              selectedLayerGroupId,
            ]),
          ],
        }
      })

      await timer.sleep()

      setState((prevState) => ({
        ...prevState,
        centeredEquipmentId: prevState.selectedAsset?.id,
      }))
    },

    getMain3dLayerForModuleTypeName(moduleTypeNamePath) {
      return getModuleForModuleTypeName(this.modules3D, moduleTypeNamePath)
    },

    getMain2dImageForModuleTypeName(moduleTypeNamePath) {
      return getModuleForModuleTypeName(this.modules2D, moduleTypeNamePath)
    },

    formatModuleTypeNamePath: convertModuleTypeNamePath,
  }

  const focusedAssetCheckRef = useRef(true)
  const isFocusedAssetSelected = context.selectedAsset?.id === assetId
  const selectedAssetQuery = useGetSelectedAsset(
    {
      siteId: site.id,
      assetId,
    },
    { enabled: site.id != null && assetId != null }
  )
  // the following effect is needed to handle the situation
  // where user initially lands on Floor Viewer (Classic Viewer)
  // page and query param string of "assetId" is defined.
  // It means user was viewing info for that asset so we need to
  // set selectedAsset to the asset matching assetId.
  // This situation will happen only once as future selecterAsset
  // change will be triggered by onClick handler attached to each
  // Asset defined in "./AssetSelector/Assets/Assets"
  useEffect(() => {
    // when assetId is no defined upon landing on Classic Viewer, it means
    // user is no focusing on any asset, we do not need this effect to run
    if (assetId == null) {
      focusedAssetCheckRef.current = false
    }

    if (focusedAssetCheckRef.current && assetId && selectedAssetQuery.data) {
      // turn on 3D module layer that contains the asset to allow 3D Viewer to display the asset
      if (context.selectedAsset?.id !== selectedAssetQuery.data.id) {
        context.selectAsset({
          ...selectedAssetQuery.data,
          moduleTypeNamePath: convertModuleTypeNamePath(
            selectedAssetQuery.data?.moduleTypeNamePath ?? ''
          ),
        })

        const layerId = context.getMain3dLayerForModuleTypeName(
          selectedAssetQuery.data.moduleTypeNamePath
        )?.id
        if (layerId != null && !context.selectedLayerIds.includes(layerId)) {
          context.setSelectedLayerIds((prevSelectedLayerIds) => [
            ...prevSelectedLayerIds,
            layerId,
          ])
        }
      }
      // request 3D viewer to select the asset
      if (
        iframeRef.current?.contentWindow?.selectAsset &&
        selectedAssetQuery.data?.forgeViewerModelId
      ) {
        iframeRef.current?.contentWindow?.selectAsset({
          assetId: selectedAssetQuery.data.id,
          forgeViewerAssetId:
            selectedAssetQuery.data?.forgeViewerModelId.toLocaleLowerCase(),
        })
        focusedAssetCheckRef.current = false
      }
    }
  }, [
    isFocusedAssetSelected,
    iframeRef.current?.contentWindow?.selectAsset,
    selectedAssetQuery.data,
  ])

  const twinIdOfSelectedAsset =
    (statsQuery?.data || [])?.find(
      (stat) =>
        stat.geometryViewerId === state?.selectedAsset?.forgeViewerModelId
    )?.twinId ?? state?.selectedAsset?.twinId

  const handleAssetIdChange = (assetId) => {
    /**
     * Setting details tab by default when asset is selected
     * Reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80296
     */
    setSearchParams({
      assetId,
      insightId: undefined,
    })
  }

  return (
    <FloorContext.Provider value={context}>
      <div
        css={css(({ theme }) => ({
          display: 'flex',
          height: '100%',
          gap: theme.spacing.s12,
        }))}
      >
        <PanelGroup>
          {floorViewType === '3D' ? (
            <AssetSelector
              siteId={site.id}
              floorId={floor.floorId}
              selectedAssetQuery={selectedAssetQuery}
              assetId={assetId}
              onSelectedAssetIdChange={handleAssetIdChange}
            />
          ) : (
            <></>
          )}
          <PanelGroup resizable>
            {state.selectedAsset ? (
              <AssetDetailPanel
                key={state.selectedAsset.id}
                siteId={site.id}
                twinIdOfSelectedAsset={twinIdOfSelectedAsset}
                asset={state.selectedAsset}
                onCloseClick={() => {
                  setSearchParams({ assetId: undefined })
                  context.selectAsset(undefined)
                  context?.iframeRef?.current?.contentWindow?.selectAsset?.(
                    undefined
                  )
                }}
                selectedDataTab={selectedDataTab}
                onSelectedDataTabChange={setSelectedDataTab}
              />
            ) : (
              // PanelGroup expects at least one child
              <></>
            )}
            <PanelGroup resizable key={floorViewType?.toString() ?? '3D'}>
              {floorViewType === 'GIS' ? (
                <Panel>
                  <EditorGIS site={site} layers={site.arcGisLayers} />
                </Panel>
              ) : isFloorEditorDisabled ? (
                <>
                  <Panel id="no-floor-plan-loaded-panel">
                    <PanelContent tw="h-full flex">
                      <NotFound>{t('plainText.noFloorPlanLoaded')}</NotFound>
                    </PanelContent>
                  </Panel>
                  <SidePanel />
                </>
              ) : (
                <>
                  <Editor />
                  {floorViewType !== 'GIS' && <SidePanel />}
                </>
              )}
            </PanelGroup>
          </PanelGroup>
        </PanelGroup>
      </div>
      {showErrorModal && (
        <ErrorModal onClose={() => setShowErrorModal(false)} />
      )}
    </FloorContext.Provider>
  )
}
